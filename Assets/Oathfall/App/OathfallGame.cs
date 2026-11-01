using Oathfall.Core;
using Oathfall.Presentation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections.Generic;
namespace Oathfall.Gameplay
{
    public sealed class OathfallGame:MonoBehaviour,IGameSession
    {
        public static OathfallGame Instance{get;private set;}
        private readonly ItemGenerator generator=new ItemGenerator();
        private uint lootSeed=0x0A7F411u;
        private RuntimeHud hud;private TopDownCamera cameraRig;private HollowwoodSceneContext scene;
        private readonly HashSet<Combatant> encounterEnemies=new HashSet<Combatant>();
        private bool encounterStarted;private int wave;private float nextWave,elapsed,flaskReady;private WardProgress ward;private LineRenderer wardRing;
        public int Flasks{get;private set;}=3;public float Elapsed=>elapsed;public int LastDefeated{get;private set;}public float LastDuration{get;private set;}
        public string Location=>ExpeditionContent.Stops[Mathf.Min(stage,ExpeditionContent.Stops.Length-1)].Name;
        public Vector3 Destination=>stage<Centers.Length?Centers[stage]:new Vector3(0,0,480);
        private Transform runRoot;private int defeated,stage,alive;private float deathAt=-1;private bool victory,cacheOpened;
        public PlayerController Player{get;private set;}
        public bool RunActive{get;private set;}
        public bool InputBlocked=>hud!=null && hud.InventoryOpen;
        public InventoryState Inventory{get;private set;}=new InventoryState();
        public int Stage=>stage;
        public string Objective{get;private set;}
        public static readonly Vector3[] Centers=ExpeditionContent.Stops.Select(s=>new Vector3(s.X,0,s.Z)).ToArray();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntime(){if(FindFirstObjectByType<OathfallGame>()==null && FindFirstObjectByType<HollowwoodSceneContext>()!=null)new GameObject("Oathfall Runtime").AddComponent<OathfallGame>();}
        private void Awake()
        {
            Instance=this;GameplaySession.Current=this;if(Soundscape.Instance==null)gameObject.AddComponent<Soundscape>();RunePulse.Fired+=OnRuneFired;Application.targetFrameRate=60;Application.runInBackground=true;
            scene=FindFirstObjectByType<HollowwoodSceneContext>();hud=RuntimeHud.Create(SelectHero);cameraRig=TopDownCamera.Create();
            if(scene!=null && scene.HeroSpawn!=null)cameraRig.SetPreview(scene.HeroSpawn.position);
            hud.ShowHeroSelection();
        }
        private static void OnRuneFired()=>Soundscape.Play(SoundCue.Rune);
        private void OnDestroy(){RunePulse.Fired-=OnRuneFired;Time.timeScale=1;if(Instance==this){Instance=null;GameplaySession.Current=null;}}
        private void OnApplicationFocus(bool focused)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-oathfall-audit")>=0)return;
#endif
            if(!focused)PauseForFocusLoss();
        }
        public void PauseForFocusLoss(){if(RunActive && Time.timeScale>0)hud.TogglePause();}
        private void Update()
        {
            var keyboard=Keyboard.current;var pad=Gamepad.current;
            if(!RunActive){if(keyboard?.digit1Key.wasPressedThisFrame==true)SelectHero(HeroId.Dawnwarden);return;}
            if((keyboard?.escapeKey.wasPressedThisFrame??false)||(pad?.startButton.wasPressedThisFrame??false))hud.TogglePause();
            if((keyboard?.iKey.wasPressedThisFrame??false)||(pad?.buttonNorth.wasPressedThisFrame??false))hud.ToggleInventory();
            if(Player==null || Player.GetComponent<Combatant>().IsDead)
            {
                if(deathAt<0){deathAt=Time.unscaledTime;hud.ClosePanels();Objective="The oath has fallen";}
                if(Time.unscaledTime-deathAt>2)EndRun(false);
                return;
            }
            hud.UpdateRun(Player,defeated,0);
            if(Time.timeScale==0)return;
            elapsed+=Time.deltaTime;
            if((keyboard?.fKey.wasPressedThisFrame??false)||(pad?.dpad.up.wasPressedThisFrame??false))UseFlask();
            if(!cacheOpened && scene.OptionalCache!=null && Vector3.Distance(Player.transform.position,scene.OptionalCache.position)<2.5f)
            {
                cacheOpened=true;Drop(scene.OptionalCache.position+Vector3.right);Drop(scene.OptionalCache.position+Vector3.left);
                hud.ShowMessage("Lost expedition cache discovered — two equipment rewards.");
            }
            if(scene.ShortcutGate!=null)scene.ShortcutGate.SetActive(stage<2);
            if(stage<Centers.Length)
            {
                var stop=ExpeditionContent.Stops[stage];bool inside=Vector3.Distance(Player.transform.position,Centers[stage])<9;
                if(!encounterStarted)
                {
                    Objective="Follow the trail to "+stop.Name;
                    if(inside)BeginEncounter();
                }
                else if(stop.Kind==EncounterKind.Ward)
                {
                    ward.Tick(Time.deltaTime,inside,true);
                    Objective=(inside?"Hold the marked ward":"Return to the marked ward")+"  "+Mathf.CeilToInt(ward.Elapsed)+" / "+stop.WardSeconds+"s • threats "+alive;
                    if(!ward.Complete&&alive<=2&&Time.time>=nextWave)SpawnWave();
                    if(ward.Complete&&alive==0)CompleteEncounter();
                }
                else if(alive==0&&Time.time>=nextWave)
                {
                    if(wave<stop.Waves)SpawnWave();else CompleteEncounter();
                }
            }
            if(victory && Vector3.Distance(Player.transform.position,new Vector3(0,0,480))<3)EndRun(true);
        }
        public void UseFlask()
        {
            if(!RunActive||Player==null||Time.timeScale==0||Flasks<=0||Time.time<flaskReady)return;
            var health=Player.GetComponent<Combatant>();if(health.IsDead||health.Health>=health.MaxHealth)return;
            health.Heal(65);Flasks--;flaskReady=Time.time+8;hud.ShowMessage("Restorative draught • "+Flasks+" remaining. Refill at secured stops.");
        }
        private void BeginEncounter()
        {
            var stop=ExpeditionContent.Stops[stage];encounterStarted=true;wave=0;
            if(stop.Kind==EncounterKind.Sanctuary){Player.GetComponent<Combatant>().Heal(1000);Flasks=3;Drop(Centers[stage]+Vector3.right*2);CompleteEncounter();hud.ShowMessage("Sanctuary reached. Health and draughts restored; compare your equipment before continuing.");return;}
            if(stop.Kind==EncounterKind.Ward)
            {
                ward=new WardProgress(stop.WardSeconds);
                var marker=new GameObject("Ward defense boundary",typeof(LineRenderer));marker.transform.SetParent(runRoot);wardRing=marker.GetComponent<LineRenderer>();wardRing.sharedMaterial=scene.Raider.TelegraphMaterial;wardRing.loop=true;wardRing.widthMultiplier=.06f;wardRing.positionCount=80;var wardColor=new MaterialPropertyBlock();wardColor.SetColor("_BaseColor",new Color(.82f,.7f,.28f));wardRing.SetPropertyBlock(wardColor);
                for(int i=0;i<80;i++){float angle=i*Mathf.PI/40;float radius=i%10==0?8.1f:8.5f;wardRing.SetPosition(i,Centers[stage]+new Vector3(Mathf.Cos(angle)*radius,.08f,Mathf.Sin(angle)*radius));}
            }
            SpawnWave();
        }
        private void SpawnWave()
        {
            var stop=ExpeditionContent.Stops[stage];wave++;nextWave=Time.time+10;
            Objective="Secure "+stop.Name+" • group "+wave+" / "+Mathf.Max(1,stop.Waves);
            int count=stop.Kind==EncounterKind.Ward?Mathf.Max(0,6-alive):stop.Count;
            for(int i=0;i<count;i++)
            {
                var role=stop.Kind==EncounterKind.Guardian?EnemyRole.Guardian:stage==0?EnemyRole.Raider:(i+wave)%4==0?EnemyRole.Hexblade:(i+wave)%4==1?EnemyRole.Breaker:EnemyRole.Raider;
                SpawnEnemy(Centers[stage]+new Vector3(Mathf.Cos((i+wave)*2.399f)*7,0,Mathf.Sin((i+wave)*2.399f)*7),i,role);
            }
        }
        private void SpawnEnemy(Vector3 position,int index,EnemyRole role)
        {
            if(!CombatSpatial.TryNavigableSpawn(position,Player.transform.position,out var spawn)){Debug.LogError("No accessible encounter spawn: "+position);return;}
            var enemy=new GameObject(role==EnemyRole.Guardian?"Fallen Oathkeeper":role.ToString());enemy.transform.SetParent(runRoot);enemy.transform.position=spawn;
            var col=enemy.AddComponent<CapsuleCollider>();col.center=Vector3.up;col.height=2;col.radius=.4f;
            var definition=scene.ForRole(role);if(definition==null)throw new System.InvalidOperationException("Missing role presentation "+role);
            var visual=enemy.AddComponent<ActorPresentationController>();visual.Initialize(definition);
            var health=enemy.AddComponent<Combatant>();health.Initialize(CombatTeam.Enemy,role==EnemyRole.Guardian?650:role==EnemyRole.Breaker?115:role==EnemyRole.Hexblade?60:75,role==EnemyRole.Breaker?20:6,Color.white);
            var feedback=enemy.AddComponent<CombatFeedbackView>();feedback.Initialize(definition.TelegraphMaterial,definition.ImpactMaterial,true);health.HitResolved+=feedback.ShowHit;
            enemy.AddComponent<NavMeshAgent>();enemy.AddComponent<EnemyBrain>().Initialize(Player.transform,index,role,definition.TelegraphMaterial);
            encounterEnemies.Add(health);alive++;
        }
        private void CompleteEncounter()
        {
            if(!encounterStarted)return;encounterStarted=false;if(wardRing!=null)Destroy(wardRing.gameObject);
            stage++;ward=null;Player.GetComponent<Combatant>().Heal(45);Flasks=Mathf.Min(3,Flasks+1);
            Soundscape.Play(SoundCue.Ward);Drop(Player.transform.position+Vector3.right*2);hud.ShowMessage("Area secured • +45 health and one draught. Choose your equipment.");
            if(stage==Centers.Length){victory=true;Objective="Hollowwood reclaimed — reach the northern waystone";}
        }
        public void OnCombatantDefeated(Combatant target)
        {
            if(!RunActive||!encounterEnemies.Remove(target))return;
            defeated++;alive=Mathf.Max(0,alive-1);
            if(defeated%4==0||target.GetComponent<EnemyBrain>().Role==EnemyRole.Guardian)Drop(target.transform.position);
            if(alive==0){nextWave=Time.time+1.25f;Player.GetComponent<Combatant>().Heal(12);}
        }
        public void Drop(Vector3 position)
        {
            var origin=position;bool placed=false;
            for(int attempt=0;attempt<25;attempt++)
            {
                float angle=attempt*Mathf.PI*.76f;float radius=attempt==0?0:1+attempt*.18f;
                var candidate=origin+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*radius;
                if(!NavMesh.SamplePosition(candidate,out var surface,2,NavMesh.AllAreas))continue;
                var ground=CombatSpatial.Ground(surface.position);
                if(Physics.CheckCapsule(ground+Vector3.up*.5f,ground+Vector3.up*1.5f,.42f,CombatSpatial.WorldMask,QueryTriggerInteraction.Ignore))continue;
                position=ground;placed=true;break;
            }
            if(!placed){Debug.LogError("No accessible reward position near "+origin);return;}
            lootSeed=lootSeed*1664525u+1013904223u;
            var item=generator.Generate(HeroId.Dawnwarden,Mathf.Clamp(1+stage/3,1,5),lootSeed);
            var obj=new GameObject("Loot "+item.Rarity+" "+item.Slot);obj.transform.SetParent(runRoot);obj.transform.position=position;
            var gear=GearCatalog.Load();
            if(gear!=null&&gear.Model(item.Slot)!=null){var display=Instantiate(gear.Model(item.Slot),obj.transform);display.transform.localScale=Vector3.one*.45f;}else if(scene.LootPrefab!=null)Instantiate(scene.LootPrefab,obj.transform);
            obj.AddComponent<LootPickup>().Initialize(item);
            obj.AddComponent<LootLabel>().Initialize(item);
        }
        public PickupResult TryCollect(ItemInstance item)
        {
            var result=Inventory.TryCollect(item);
            if(result==PickupResult.Collected){Soundscape.Play(SoundCue.Loot);hud.ShowMessage("Collected "+item.Rarity+" "+item.Slot+" — I / Y: inventory");hud.RefreshInventory();}
            else if(result==PickupResult.Full)hud.ShowMessage("Inventory full. Equip an item to free space; the drop remains here.");
            return result;
        }
        public void Discard(int index){if(Inventory.Discard(index)){hud.RefreshInventory();hud.ShowMessage("Unequipped item discarded.");}}
        public void Equip(int index){if(Inventory.Equip(index)){Soundscape.Play(SoundCue.Equip);Player.SetStats(EquipmentStats.Calculate(Inventory.Equipment));hud.RefreshInventory();}}
        public void Unequip(ItemSlot slot){if(Inventory.Unequip(slot)){Soundscape.Play(SoundCue.Equip);Player.SetStats(EquipmentStats.Calculate(Inventory.Equipment));hud.RefreshInventory();}else hud.ShowMessage("No free inventory space.");}
        public void SelectHero(HeroId hero)
        {
            if(hero!=HeroId.Dawnwarden || scene==null)return;
            ClearRun();Inventory=new InventoryState();RunActive=true;stage=0;alive=0;defeated=0;deathAt=-1;victory=false;cacheOpened=false;encounterStarted=false;wave=0;ward=null;elapsed=0;Flasks=3;flaskReady=0;encounterEnemies.Clear();
            if(scene.ShortcutGate!=null)scene.ShortcutGate.SetActive(true);
            runRoot=new GameObject("Expedition Actors").transform;hud.ClosePanels();hud.HideHeroSelection();
            var player=new GameObject("Dawnwarden");player.transform.SetParent(runRoot);player.transform.position=scene.HeroSpawn.position;
            var controller=player.AddComponent<CharacterController>();controller.center=Vector3.up;controller.height=2;controller.radius=.38f;controller.stepOffset=.35f;
            player.AddComponent<ActorPresentationController>().Initialize(scene.Dawnwarden);
            player.AddComponent<Combatant>().Initialize(CombatTeam.Hero,150,22,Color.white);
            var feedback=player.AddComponent<CombatFeedbackView>();feedback.Initialize(scene.Dawnwarden.TelegraphMaterial,scene.Dawnwarden.ImpactMaterial,false);player.GetComponent<Combatant>().HitResolved+=feedback.ShowHit;
            Player=player.AddComponent<PlayerController>();Player.Initialize(hero);cameraRig.Target=player.transform;cameraRig.SetPreview(player.transform.position);
            player.GetComponent<Combatant>().Damaged+=cameraRig.Impulse;
            hud.ShowMessage("Follow the old road. Q: Shield Bash  E: Cleaving Arc  I: Equipment");
        }
        private void EndRun(bool won){LastDuration=elapsed;LastDefeated=defeated;Debug.Log("EXPEDITION result="+(won?"victory":"death")+" seconds="+elapsed.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+" defeated="+defeated);if(won)Soundscape.Play(SoundCue.Victory);RunActive=false;hud.ClosePanels();ClearRun();hud.ShowHeroSelection(won?"Hollowwood reclaimed in "+System.TimeSpan.FromSeconds(LastDuration).ToString(@"mm\:ss")+" • "+LastDefeated+" foes defeated.":"You fell in Hollowwood. Begin a new expedition.");}
        private void ClearRun(){if(runRoot!=null){runRoot.gameObject.SetActive(false);Destroy(runRoot.gameObject);}Player=null;}
    }
}
