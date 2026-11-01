using System.Collections;
using System.Linq;
using NUnit.Framework;
using Oathfall.Core;
using Oathfall.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Oathfall.Tests.PlayMode
{
    public class ExpeditionTests
    {
        private OathfallGame game;
        private Gamepad pad;
        private Keyboard testKeyboard;
        [UnityTearDown] public IEnumerator Teardown(){if(pad!=null)InputSystem.RemoveDevice(pad);if(testKeyboard!=null)InputSystem.RemoveDevice(testKeyboard);testKeyboard=null;pad=null;Time.timeScale=1;yield return null;}
        [UnitySetUp] public IEnumerator Setup()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Hollowwood");
            yield return null;
            game=Object.FindFirstObjectByType<OathfallGame>();
            if(game==null)game=new GameObject("Test Composition").AddComponent<OathfallGame>();
            game.SelectHero(HeroId.Dawnwarden);yield return null;
        }
        [UnityTest] public IEnumerator KeyboardCanCloseEquipmentAndPauseAtZeroTimeScale()
        {
            testKeyboard=InputSystem.AddDevice<Keyboard>();var hud=Object.FindFirstObjectByType<RuntimeHud>();
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.I));yield return null;yield return null;
            Assert.That(hud.InventoryOpen,Is.True);Assert.That(Time.timeScale,Is.Zero);
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return null;yield return null;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.I));yield return null;yield return null;
            Assert.That(hud.InventoryOpen,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return null;yield return null;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.Escape));yield return null;yield return null;Assert.That(Time.timeScale,Is.Zero);
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return null;yield return null;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.Escape));yield return null;yield return null;Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ThreeCombosUseDistinctTrajectoriesAndMatchVisibleBlade()
        {
            game.Player.enabled=false;var view=game.Player.GetComponent<Oathfall.Presentation.ActorPresentationController>();var lib=view.Definition.WeaponMotion;
            for(int combo=0;combo<3;combo++)
            {
                view.PlayAttack(combo,0,combo==2?.84f:.68f);view.SetAttackProgress(.5f);yield return null;yield return null;
                var animator=game.Player.GetComponentInChildren<Animator>();Assert.That(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name,Is.EqualTo("Dawnwarden_Combo"+(combo+1)));
                view.SampleBlade(.5f,out _,out var tip);var point=game.Player.transform.TransformPoint(new Vector3(tip.X,tip.Y,tip.Z));var expected=animator.GetBoneTransform(HumanBodyBones.RightHand).TransformPoint(lib.handLocalTip);
                Assert.That(Vector3.Distance(point,expected),Is.LessThan(.08f));Assert.That(tip.Z,Is.GreaterThan(.85f));Assert.That(tip.Y,Is.InRange(.5f,1.9f));
            }
            Assert.That(Vector3.Distance(lib.motions[0].tips[36],lib.motions[1].tips[36]),Is.GreaterThan(1));
            Assert.That(lib.motions[2].tips[36].y,Is.GreaterThan(lib.motions[0].tips[36].y+.6f));
        }
        [UnityTest] public IEnumerator EquipmentInspectionIsReadOnlyAndStudioSuspendsWhenClosed()
        {
            var item=new ItemInstance{InstanceId="preview-item",Slot=ItemSlot.Charm,ItemLevel=3};game.TryCollect(item);
            var hud=Object.FindFirstObjectByType<RuntimeHud>();hud.ToggleInventory();Assert.That(hud.IsRunHudVisible,Is.False,"Modal must hide combat UI in the opening frame.");yield return null;
            var preview=Object.FindFirstObjectByType<GearPreview>();Assert.That(preview,Is.Not.Null);Assert.That(preview.ShownSlot,Is.EqualTo(ItemSlot.Charm));Assert.That(preview.HasSubject,Is.True);
            Assert.That(game.Inventory.Items.Count,Is.EqualTo(1));Assert.That(game.Inventory.Equipment[ItemSlot.Charm],Is.Null);Assert.That(Time.timeScale,Is.Zero);
            var camera=Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Single(c=>c.name=="Equipment preview camera");Assert.That(camera.targetTexture.IsCreated(),Is.True);
            game.Equip(0);yield return null;Assert.That(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject,Is.Not.Null);
            hud.ToggleInventory();yield return null;Assert.That(camera.gameObject.activeInHierarchy,Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
            hud.ToggleInventory();yield return null;Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c=>c.name=="Equipment preview camera"),Is.EqualTo(1));hud.ToggleInventory();
        }
        [UnityTest] public IEnumerator GroundedPickupAddsOnceAndEquipmentChangesDamage()
        {
            var initial=game.Player.Stats.Damage;
            var item=new ItemInstance{InstanceId="pickup-regression",Slot=ItemSlot.Weapon,ItemLevel=2};
            var obj=new GameObject("Height regression drop");obj.transform.position=game.Player.transform.position+Vector3.up*1.45f;
            obj.AddComponent<LootPickup>().Initialize(item);
            yield return new WaitForSeconds(.15f);
            Assert.That(game.Inventory.Items.Count,Is.EqualTo(1));Assert.That(obj==null,Is.True);
            Assert.That(game.TryCollect(item),Is.EqualTo(PickupResult.Duplicate));
            game.Equip(0);Assert.That(game.Player.Stats.Damage,Is.GreaterThan(initial));
            game.Unequip(ItemSlot.Weapon);Assert.That(game.Player.Stats.Damage,Is.EqualTo(initial));
        }
        [UnityTest] public IEnumerator SpawnReachabilityProjectsPlayerStandingOutsideAgentClearance()
        {
            // Reproduced from the second revision-5 standalone trace at 332.30s.
            var playerEdge=new Vector3(-14.60427f,0,174.7434f);
            foreach(var origin in new[]{new Vector3(-22.09f,0,167.63f),new Vector3(-22.83f,0,180.66f),new Vector3(-31.10f,0,170.56f),new Vector3(-18.18f,0,172.41f)})
            {
                Assert.That(CombatSpatial.TryNavigableSpawn(origin,playerEdge,out var spawn),Is.True,"Player can stand just outside the larger NavMesh agent clearance.");
                Assert.That(Physics.CheckCapsule(spawn+Vector3.up*.5f,spawn+Vector3.up*1.5f,.4f,CombatSpatial.WorldMask),Is.False);
            }
            Assert.That(CombatSpatial.TryNavigableSpawn(Vector3.zero,new Vector3(10000,0,10000),out _),Is.False,"Projection must remain bounded.");
            yield return null;
        }
        [UnityTest] public IEnumerator EncounterSpawnsStayOnConnectedGround()
        {
            foreach(var modifier in Object.FindObjectsByType<Unity.AI.Navigation.NavMeshModifier>(FindObjectsSortMode.None).Where(m=>m.name.Contains("Boulder")))
                Assert.That(modifier.gameObject.layer,Is.EqualTo(8),"Boulder exclusion must be included in the surface's layer mask.");
            foreach(var stop in ExpeditionContent.Stops.Where(s=>s.Kind!=EncounterKind.Sanctuary))
                for(int wave=1;wave<=4;wave++)for(int i=0;i<6;i++)
                {
                    var origin=new Vector3(stop.X+Mathf.Cos((i+wave)*2.399f)*7,0,stop.Z+Mathf.Sin((i+wave)*2.399f)*7);
                    Assert.That(CombatSpatial.TryNavigableSpawn(origin,new Vector3(stop.X,0,stop.Z),out var spawn),Is.True,stop.Id+" wave "+wave+" enemy "+i);
                    Assert.That(spawn.y,Is.LessThan(.5f),"Spawn selected a roof or boulder instead of the encounter floor.");
                    Assert.That(Physics.CheckCapsule(spawn+Vector3.up*.5f,spawn+Vector3.up*1.5f,.4f,CombatSpatial.WorldMask),Is.False);
                }
            yield return null;
        }
        [UnityTest] public IEnumerator EntireRouteIsTraversableByPlayerCollider()
        {
            game.Player.enabled=false;var controller=game.Player.GetComponent<CharacterController>();
            var goals=ExpeditionContent.Stops.Select(s=>new Vector3(s.X,0,s.Z)).Concat(new[]{new Vector3(0,0,480)});
            foreach(var goal in goals)
            {
                var path=new UnityEngine.AI.NavMeshPath();
                Assert.That(UnityEngine.AI.NavMesh.CalculatePath(game.Player.transform.position,goal,UnityEngine.AI.NavMesh.AllAreas,path),Is.True);
                Assert.That(path.status,Is.EqualTo(UnityEngine.AI.NavMeshPathStatus.PathComplete));
                foreach(var corner in path.corners.Skip(1))
                {
                    for(int step=0;step<2500;step++)
                    {
                        var delta=corner-game.Player.transform.position;delta.y=0;if(delta.magnitude<.15f)break;
                        controller.Move(delta.normalized*.05f+Vector3.down*.03f);
                    }
                    var remaining=corner-game.Player.transform.position;remaining.y=0;
                    Assert.That(remaining.magnitude,Is.LessThan(.2f),"Physical route blocked towards "+goal+" near "+corner+" player "+game.Player.transform.position);
                }
            }
            yield return null;
        }
        [UnityTest] public IEnumerator GroveRockRouteIsTraversableByPlayerCollider()
        {
            game.Player.enabled=false;
            var controller=game.Player.GetComponent<CharacterController>();
            Assert.That(UnityEngine.AI.NavMesh.GetSettingsByID(0).agentClimb,Is.LessThanOrEqualTo(controller.stepOffset));
            Assert.That(UnityEngine.AI.NavMesh.SamplePosition(new Vector3(-11.28f,0,172.91f),out var start,3,UnityEngine.AI.NavMesh.AllAreas),Is.True);
            var goal=new Vector3(-25,0,174);var path=new UnityEngine.AI.NavMeshPath();
            Assert.That(UnityEngine.AI.NavMesh.CalculatePath(start.position,goal,UnityEngine.AI.NavMesh.AllAreas,path),Is.True);
            Assert.That(path.status,Is.EqualTo(UnityEngine.AI.NavMeshPathStatus.PathComplete));
            controller.enabled=false;game.Player.transform.position=start.position+Vector3.up*.05f;controller.enabled=true;
            foreach(var corner in path.corners.Skip(1))
            {
                for(int step=0;step<600;step++)
                {
                    var delta=corner-game.Player.transform.position;delta.y=0;if(delta.magnitude<.15f)break;
                    controller.Move(delta.normalized*.05f+Vector3.down*.03f);
                }
                var remaining=corner-game.Player.transform.position;remaining.y=0;
                Assert.That(remaining.magnitude,Is.LessThan(.2f),"Collider blocked a baked navigation segment near "+corner);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator FullInventoryDoesNotDestroyDrop()
        {
            for(int i=0;i<24;i++)game.TryCollect(new ItemInstance{InstanceId="full"+i});
            var obj=new GameObject("Retained loot");obj.transform.position=game.Player.transform.position;
            obj.AddComponent<LootPickup>().Initialize(new ItemInstance{InstanceId="overflow"});
            yield return new WaitForSeconds(.2f);Assert.That(obj!=null,Is.True);Assert.That(game.Inventory.Items.Count,Is.EqualTo(24));
            Object.Destroy(obj);
        }
        [UnityTest] public IEnumerator WallPreventsPickup()
        {
            var p=game.Player.transform.position;
            var wall=new GameObject("Pickup occlusion wall");wall.layer=8;wall.transform.position=p+Vector3.right*.55f+Vector3.up;
            var collider=wall.AddComponent<BoxCollider>();collider.size=new Vector3(.1f,2,2);
            var obj=new GameObject("Occluded loot");obj.transform.position=p+Vector3.right*1.1f;
            obj.AddComponent<LootPickup>().Initialize(new ItemInstance{InstanceId="wall"});Physics.SyncTransforms();
            yield return new WaitForSeconds(.2f);Assert.That(game.Inventory.Items.Count,Is.Zero);Assert.That(obj!=null,Is.True);
            Object.Destroy(wall);Object.Destroy(obj);
        }
        [UnityTest] public IEnumerator RestartDisablesPreviousRunBeforeNextFrame()
        {
            var old=game.Player;game.SelectHero(HeroId.Dawnwarden);
            Assert.That(old.gameObject.activeInHierarchy,Is.False);yield return null;
            Assert.That(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            Assert.That(game.Player.GetComponent<Combatant>().Health,Is.EqualTo(150));
        }
        [UnityTest] public IEnumerator HeroDeathFinishesPresentationThenAllowsCleanExpedition()
        {
            game.Player.GetComponent<Combatant>().ReceiveDamage(new DamageRequest{RawAmount=10000});
            yield return null;
            Assert.That(game.RunActive,Is.True,"Death presentation should finish before the result menu.");
            yield return new WaitForSecondsRealtime(2.3f);
            Assert.That(game.RunActive,Is.False);Assert.That(game.Player==null,Is.True);
            game.SelectHero(HeroId.Dawnwarden);yield return null;
            Assert.That(game.Stage,Is.Zero);Assert.That(game.Flasks,Is.EqualTo(3));Assert.That(game.Inventory.Items,Is.Empty);
            Assert.That(game.Player.GetComponent<Combatant>().Health,Is.EqualTo(150));
            Assert.That(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator EachEncounterCanBeReachedAndCompletedWithoutDuplicateRewards()
        {
            for(int stage=0;stage<OathfallGame.Centers.Length;stage++)
            {
                var controller=game.Player.GetComponent<CharacterController>();controller.enabled=false;game.Player.transform.position=OathfallGame.Centers[stage]+Vector3.up*.1f;controller.enabled=true;
                yield return new WaitForSeconds(.1f);
                int guard=0;
                while(game.Stage==stage&&guard++<7)
                {
                    var wardField=typeof(OathfallGame).GetField("ward",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
                    (wardField.GetValue(game) as WardProgress)?.Tick(1000,true,true); // Transition test, not pacing evidence.
                    var enemies=Object.FindObjectsByType<Combatant>(FindObjectsSortMode.None).Where(e=>e.Team==CombatTeam.Enemy&&!e.IsDead).ToArray();
                    foreach(var enemy in enemies){enemy.ReceiveDamage(new DamageRequest{RawAmount=10000});enemy.ReceiveDamage(new DamageRequest{RawAmount=10000});}
                    yield return new WaitForSeconds(1.4f);
                }
                Assert.That(game.Stage,Is.EqualTo(stage+1),"Each stop completes once after its authored groups/objective.");
            }
            Assert.That(game.Objective,Does.Contain("Hollowwood reclaimed"));
            var exitPath=new UnityEngine.AI.NavMeshPath();Assert.That(UnityEngine.AI.NavMesh.CalculatePath(game.Player.transform.position,new Vector3(0,0,480),UnityEngine.AI.NavMesh.AllAreas,exitPath),Is.True);Assert.That(exitPath.status,Is.EqualTo(UnityEngine.AI.NavMeshPathStatus.PathComplete));
            var exitController=game.Player.GetComponent<CharacterController>();exitController.enabled=false;game.Player.transform.position=new Vector3(0,.1f,480);exitController.enabled=true;
            yield return null;Assert.That(game.RunActive,Is.False);game.SelectHero(HeroId.Dawnwarden);yield return null;Assert.That(game.Stage,Is.Zero);Assert.That(game.Inventory.Items.Count,Is.Zero);
        }
        [UnityTest] public IEnumerator SlowFrameHitsOnceAndStaggerCancelsRemainingWindow()
        {
            pad=InputSystem.AddDevice<Gamepad>();var p=game.Player;
            var target=new GameObject("Sweep regression target");target.transform.position=p.transform.position+Vector3.forward*1.1f;
            var shape=target.AddComponent<CapsuleCollider>();shape.center=Vector3.up;shape.height=2;shape.radius=.4f;
            var health=target.AddComponent<Combatant>();health.Initialize(CombatTeam.Enemy,500,0,Color.white);Physics.SyncTransforms();
            InputSystem.QueueStateEvent(pad,new GamepadState{rightStick=Vector2.up,rightTrigger=1});yield return null;yield return null;
            Assert.That(p.AttackPhase,Is.Not.EqualTo(AttackPhase.None));
            p.TickAttack(.5f);Assert.That(health.Health,Is.EqualTo(474).Within(.01f));p.TickAttack(.01f);Assert.That(health.Health,Is.EqualTo(474).Within(.01f));
            p.Stagger(1);p.TickAttack(1);Assert.That(p.AttackPhase,Is.EqualTo(AttackPhase.None));Assert.That(health.Health,Is.EqualTo(474).Within(.01f));Object.Destroy(target);
        }
        [UnityTest] public IEnumerator StationaryDodgeUsesFacingAndPreventsDamageImmediately()
        {
            pad=InputSystem.AddDevice<Gamepad>();var p=game.Player;var start=p.transform.position;var health=p.GetComponent<Combatant>();
            InputSystem.QueueStateEvent(pad,new GamepadState{rightStick=Vector2.right}.WithButton(GamepadButton.South));yield return null;yield return null;
            Assert.That(p.IsDodging,Is.True);health.ReceiveDamage(new DamageRequest{RawAmount=100});Assert.That(health.Health,Is.EqualTo(150));
            yield return new WaitForSeconds(.1f);Assert.That(p.transform.position.x,Is.GreaterThan(start.x+.5f));
        }
        [UnityTest] public IEnumerator ShrineRewardIsReachableAndCollectible()
        {
            game.Drop(new Vector3(0,0,35));var drop=Object.FindFirstObjectByType<LootPickup>();
            Assert.That(Physics.CheckCapsule(drop.transform.position+Vector3.up*.5f,drop.transform.position+Vector3.up*1.5f,.38f,CombatSpatial.WorldMask),Is.False,"Reward must allow the player's collider to reach it, not merely pass a NavMesh query.");
            var controller=game.Player.GetComponent<CharacterController>();controller.enabled=false;game.Player.transform.position=drop.transform.position;controller.enabled=true;
            yield return new WaitForSeconds(.1f);Assert.That(game.Inventory.Items.Count,Is.EqualTo(1));Assert.That(drop==null,Is.True);
        }
        [UnityTest] public IEnumerator LootUnderArchStaysOnGroundRatherThanRoof()
        {
            game.Drop(new Vector3(0,0,17));var drop=Object.FindFirstObjectByType<LootPickup>();
            Assert.That(drop.transform.position.y,Is.LessThan(.5f));
            var controller=game.Player.GetComponent<CharacterController>();controller.enabled=false;game.Player.transform.position=drop.transform.position;controller.enabled=true;
            yield return new WaitForSeconds(.1f);Assert.That(game.Inventory.Items.Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator TenRestartsLeaveOneActorSetAndControllerInventoryFocusWorks()
        {
            for(int i=0;i<10;i++){game.SelectHero(HeroId.Dawnwarden);yield return null;}
            Assert.That(Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).Length,Is.EqualTo(1));
            game.TryCollect(new ItemInstance{InstanceId="focus",Slot=ItemSlot.Weapon});pad=InputSystem.AddDevice<Gamepad>();
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.North));yield return null;yield return null;
            Assert.That(Time.timeScale,Is.Zero);Assert.That(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject,Is.Not.Null);
        }
        [UnityTest] public IEnumerator FocusLossPausesCombatUntilExplicitResume()
        {
            game.PauseForFocusLoss();yield return null;Assert.That(Time.timeScale,Is.Zero);
            game.PauseForFocusLoss();Assert.That(Time.timeScale,Is.Zero);
            Object.FindFirstObjectByType<RuntimeHud>().TogglePause();Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DodgeRecoveryRemainsVulnerableAfterProtectionEnds()
        {
            pad=InputSystem.AddDevice<Gamepad>();var player=game.Player;var health=player.GetComponent<Combatant>();
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());
            yield return new WaitForSeconds(.29f);
            Assert.That(player.IsDodging,Is.True,"Longer visual recovery must not extend invulnerability.");
            health.ReceiveDamage(new DamageRequest{RawAmount=20});Assert.That(health.Health,Is.LessThan(150));
            yield return new WaitForSeconds(.18f);Assert.That(player.IsDodging,Is.False);
        }
        [UnityTest] public IEnumerator ImmuneAndDeadTargetsDoNotEmitFalseImpactFeedback()
        {
            var health=game.Player.GetComponent<Combatant>();int feedback=0;health.HitResolved+=(h,m,d)=>feedback++;
            health.IncomingDamageMultiplier=0;health.ReceiveDamage(new DamageRequest{RawAmount=20});Assert.That(feedback,Is.Zero);
            health.IncomingDamageMultiplier=1;health.ReceiveDamage(new DamageRequest{RawAmount=10000});Assert.That(feedback,Is.EqualTo(1));
            health.ReceiveDamage(new DamageRequest{RawAmount=20});Assert.That(feedback,Is.EqualTo(1));yield return null;
        }
        [UnityTest] public IEnumerator EquipmentMutationClearsStaleDiscardSelection()
        {
            game.TryCollect(new ItemInstance{InstanceId="ui-discard-a",Slot=ItemSlot.Weapon});game.TryCollect(new ItemInstance{InstanceId="ui-discard-b",Slot=ItemSlot.Body});
            var hud=Object.FindFirstObjectByType<RuntimeHud>();var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            typeof(RuntimeHud).GetMethod("Compare",flags).Invoke(hud,new object[]{0});game.Equip(0);
            Assert.That((int)typeof(RuntimeHud).GetField("selectedBagIndex",flags).GetValue(hud),Is.EqualTo(-1));Assert.That(game.Inventory.Items.Single().InstanceId,Is.EqualTo("ui-discard-b"));yield return null;
        }
        [UnityTest] public IEnumerator StaggerCancelsTelegraphedSpellAndFlasksAreLimited()
        {
            var enemy=new GameObject("Spell interruption",typeof(Combatant),typeof(UnityEngine.AI.NavMeshAgent),typeof(EnemyBrain));
            enemy.GetComponent<Combatant>().Initialize(CombatTeam.Enemy,50,0,Color.white);
            var brain=enemy.GetComponent<EnemyBrain>();var health=game.Player.GetComponent<Combatant>();
            RunePulse.Create(brain,game.Player.transform,game.Player.transform.position,2,.15f,40,game.Player.GetComponent<Oathfall.Presentation.ActorPresentationController>().Definition.TelegraphMaterial);
            brain.Stagger(1);yield return new WaitForSeconds(.2f);Assert.That(health.Health,Is.EqualTo(150));
            game.UseFlask();Assert.That(game.Flasks,Is.EqualTo(3));health.ReceiveDamage(new DamageRequest{RawAmount=80});game.UseFlask();Assert.That(game.Flasks,Is.EqualTo(2));game.UseFlask();Assert.That(game.Flasks,Is.EqualTo(2));Object.Destroy(enemy);
        }
        [UnityTest] public IEnumerator SwordDoesNotHitBeyondVisibleBladeReach()
        {
            pad=InputSystem.AddDevice<Gamepad>();var p=game.Player;var target=new GameObject("Beyond blade",typeof(CapsuleCollider),typeof(Combatant));target.transform.position=p.transform.position+Vector3.forward*2.7f;var shape=target.GetComponent<CapsuleCollider>();shape.center=Vector3.up;shape.height=2;shape.radius=.4f;var health=target.GetComponent<Combatant>();health.Initialize(CombatTeam.Enemy,500,0,Color.white);Physics.SyncTransforms();
            InputSystem.QueueStateEvent(pad,new GamepadState{rightStick=Vector2.up,rightTrigger=1});yield return null;yield return null;p.TickAttack(.5f);Assert.That(health.Health,Is.EqualTo(500));Object.Destroy(target);
        }
        [UnityTest] public IEnumerator SampledBladeMatchesVisibleWeaponAndHasNoPhantomReach()
        {
            game.Player.enabled=false;var view=game.Player.GetComponent<Oathfall.Presentation.ActorPresentationController>();
            view.PlayAttack(0,0,.68f);view.SetAttackProgress(.5f);yield return null;yield return null;
            Assert.That(view.SampleBlade(.5f,out var grip,out var tip),Is.True);
            var animator=game.Player.GetComponentInChildren<Animator>();var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            var expected=hand.TransformPoint(view.Definition.WeaponMotion.handLocalTip);
            var actual=game.Player.transform.TransformPoint(new Vector3(tip.X,tip.Y,tip.Z));
            Assert.That(Vector3.Distance(actual,expected),Is.LessThan(.08f),"Gameplay contact must follow the retargeted blade, including model orientation/import scale.");
            Assert.That(Vector3.Distance(actual,game.Player.transform.position),Is.LessThan(3));
        }
        [UnityTest] public IEnumerator CleaveAnimationUsesOneSwingAtGameplayDuration()
        {
            game.Player.enabled=false;var visual=game.Player.GetComponent<Oathfall.Presentation.ActorPresentationController>();
            var animator=game.Player.GetComponentInChildren<Animator>();visual.PlayAttack(2,2,.84f);visual.SetAttackProgress(.3f);
            yield return new WaitForSeconds(.25f);
            Assert.That(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name,Does.EndWith("Sword_Attack"));
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).normalizedTime,Is.InRange(.2f,.45f));
            visual.SetAction(PresentationAction.Death);yield return new WaitForSeconds(.15f);
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Death"),Is.True);
            foreach(var trail in visual.GetComponentsInChildren<TrailRenderer>())Assert.That(trail.emitting,Is.False);
        }
        [UnityTest] public IEnumerator CameraCutawayPreservesCollisionAndRestoresVisibility()
        {
            var obj=new GameObject("Camera obstruction regression",typeof(MeshRenderer),typeof(BoxCollider));
            var renderer=obj.GetComponent<MeshRenderer>();var collider=obj.GetComponent<BoxCollider>();
            var hiddenChild=new GameObject("Already hidden",typeof(MeshRenderer));hiddenChild.transform.SetParent(obj.transform);hiddenChild.GetComponent<MeshRenderer>().enabled=false;
            var occluder=obj.AddComponent<Oathfall.Presentation.CameraOccluder>();occluder.Conceal(.05f);
            Assert.That(renderer.enabled,Is.False);Assert.That(collider.enabled,Is.True);
            yield return new WaitForSeconds(.1f);Assert.That(renderer.enabled,Is.True);Assert.That(hiddenChild.GetComponent<MeshRenderer>().enabled,Is.False);Object.Destroy(obj);
        }
        [UnityTest] public IEnumerator VisualCanopyWithoutColliderRevealsAndRestores()
        {
            var canopy=GameObject.CreatePrimitive(PrimitiveType.Cube);canopy.SetActive(false);
            canopy.transform.position=new Vector3(100,3,100);Object.Destroy(canopy.GetComponent<Collider>());
            var occluder=canopy.AddComponent<Oathfall.Presentation.CameraOccluder>();occluder.UseVisualBounds=true;canopy.SetActive(true);
            yield return null;
            Oathfall.Presentation.CameraOccluder.RevealCanopies(new Vector3(100,3,95),new Vector3(100,3,105));
            Assert.That(canopy.GetComponent<Renderer>().enabled,Is.False,"Foliage without canopy physics still needs visibility checks.");
            yield return new WaitForSeconds(.25f);Assert.That(canopy.GetComponent<Renderer>().enabled,Is.True);
            canopy.SetActive(false);Oathfall.Presentation.CameraOccluder.RevealCanopies(new Vector3(100,3,95),new Vector3(100,3,105));
            Object.Destroy(canopy);
        }
        [UnityTest] public IEnumerator CameraRevealsPlayerBodyBehindAuthoredEntranceArch()
        {
            var controller=game.Player.GetComponent<CharacterController>();controller.enabled=false;game.Player.transform.position=new Vector3(0,.1f,-32);controller.enabled=true;
            var camera=Object.FindFirstObjectByType<TopDownCamera>();camera.SetPreview(game.Player.transform.position);
            yield return null;yield return null;
            var arch=Object.FindObjectsByType<Oathfall.Presentation.CameraOccluder>(FindObjectsSortMode.None).First(o=>!o.UseVisualBounds&&o.transform.position.z<-30);
            Assert.That(arch.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),Is.True,"Clear head visibility must not leave the lower body hidden by the arch.");
            Assert.That(arch.GetComponentsInChildren<Collider>().All(c=>c.enabled),Is.True);
            controller.enabled=false;game.Player.transform.position=new Vector3(0,.1f,-43);controller.enabled=true;camera.SetPreview(game.Player.transform.position);
            yield return new WaitForSeconds(.25f);Assert.That(arch.GetComponentsInChildren<Renderer>().All(r=>r.enabled),Is.True);
        }
    }
}
