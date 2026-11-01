#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace Oathfall.Gameplay
{
    /// <summary>Opt-in synthetic input audit. Never enabled in a normal player session.</summary>
    public sealed class RuntimeAuditDriver:MonoBehaviour
    {
        private Gamepad pad;private float started,nextSample;private StreamWriter report;private string reportFolder;private NavMeshPath path;private int deaths,previousStage=-1;private bool active;private int completedRuns;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStart(){if(Environment.GetCommandLineArgs().Contains("-oathfall-audit"))new GameObject("Synthetic input audit").AddComponent<RuntimeAuditDriver>();}
        private void Start()
        {
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            path=new NavMeshPath();pad=InputSystem.AddDevice<Gamepad>();started=Time.unscaledTime;active=true;
            reportFolder=Path.Combine(Application.persistentDataPath,Environment.GetCommandLineArgs().Contains("-oathfall-audit-ui")?"Audit-UI":"Audit");Directory.CreateDirectory(reportFolder);
            report=new StreamWriter(Path.Combine(reportFolder,"input-audit.csv"));report.WriteLine("elapsed,frame,stage,health,items,x,z,frame_ms,allocated_bytes");
            Debug.Log("AUDIT synthetic Gamepad input started; no damage, position or health overrides");
        }
        private void Update()
        {
            if(!active)return;var game=OathfallGame.Instance;if(game==null)return;
            if(!game.RunActive)
            {
                if(previousStage>=Oathfall.Core.ExpeditionContent.Stops.Length){completedRuns++;Debug.Log("AUDIT victory run "+completedRuns);if(!Environment.GetCommandLineArgs().Contains("-oathfall-soak")||completedRuns>=10){Finish("victory");return;}previousStage=-1;deaths=0;}
                if(Time.unscaledTime-started<1)return;
                if(previousStage>=0){deaths++;Debug.Log("AUDIT death "+deaths);previousStage=-1;}
                if(deaths>=3){Finish("three deaths");return;}
                game.SelectHero(Oathfall.Core.HeroId.Dawnwarden);return;
            }
            var hero=game.Player;if(hero==null || hero.GetComponent<Combatant>().IsDead)return;
            if(Time.unscaledTime-started>(Environment.GetCommandLineArgs().Contains("-oathfall-soak")?10800:1200)){Finish("time limit");return;}
            if(game.Stage!=previousStage){previousStage=game.Stage;ScreenCapture.CaptureScreenshot(Path.Combine(reportFolder,"stage-"+previousStage+".png"));Debug.Log("AUDIT stage "+previousStage);}
            var enemies=FindObjectsByType<Combatant>(FindObjectsSortMode.None).Where(e=>e.Team==CombatTeam.Enemy&&!e.IsDead).OrderBy(e=>(e.transform.position-hero.transform.position).sqrMagnitude).ToArray();
            if(enemies.Length==0 && game.Inventory.Items.Count>0 && Environment.GetCommandLineArgs().Contains("-oathfall-audit-ui")){FindFirstObjectByType<RuntimeHud>().ToggleInventory();ScreenCapture.CaptureScreenshot(Path.Combine(reportFolder,"equipment-"+Screen.width+"x"+Screen.height+".png"));Finish("UI review checkpoint");return;}
            Vector3 goal;Vector3 aim=hero.transform.forward;bool dodge=false,attack=false,bash=false,cleave=false;
            if(enemies.Length>0)
            {
                var enemy=enemies[0];var delta=enemy.transform.position-hero.transform.position;delta.y=0;aim=delta.normalized;
                goal=delta.magnitude>1.05f?enemy.transform.position:hero.transform.position;
                bool threat=enemies.Any(e=>(e.transform.position-hero.transform.position).sqrMagnitude<9 && e.GetComponent<Oathfall.Presentation.ActorPresentationController>().Action==Oathfall.Core.PresentationAction.AttackWindup);
                dodge=threat && Time.time%1.0f<.12f;
                if(dodge)goal=hero.transform.position-aim*3;
                attack=delta.magnitude<1.65f && Time.time%.19f<.06f;
                bash=delta.magnitude<2.5f && hero.BashCooldown==0;
                cleave=enemies.Length>1 && delta.magnitude<3 && hero.CleaveCooldown==0;
            }
            else
            {
                var loot=FindObjectsByType<LootPickup>(FindObjectsSortMode.None).Where(l=>(l.transform.position-hero.transform.position).sqrMagnitude<100).OrderBy(l=>(l.transform.position-hero.transform.position).sqrMagnitude).FirstOrDefault();
                goal=loot!=null?loot.transform.position:game.Destination;
                for(int i=game.Inventory.Items.Count-1;i>=0;i--){var item=game.Inventory.Items[i];var equipped=game.Inventory.Equipment[item.Slot];float value=item.ItemLevel*2+item.Affixes.Sum(a=>a.Value);float old=equipped==null?-1:equipped.ItemLevel*2+equipped.Affixes.Sum(a=>a.Value);if(value>old)game.Equip(i);else game.Discard(i);}
            }
            if(game.Stage<Oathfall.Core.ExpeditionContent.Stops.Length&&Oathfall.Core.ExpeditionContent.Stops[game.Stage].Kind==Oathfall.Core.EncounterKind.Ward&&Vector3.Distance(hero.transform.position,game.Destination)>7.5f)goal=game.Destination;
            var pulse=FindObjectsByType<RunePulse>(FindObjectsSortMode.None).FirstOrDefault(p=>p.Threatens(hero.transform.position));
            if(pulse!=null){goal=hero.transform.position+(hero.transform.position-pulse.transform.position+Vector3.right*.3f).normalized*4;dodge=true;}
            var move=goal-hero.transform.position;move.y=0;
            if(move.magnitude>.1f && NavMesh.CalculatePath(hero.transform.position,goal,NavMesh.AllAreas,path)&&path.corners.Length>1)
            {
                int corner=1;while(corner<path.corners.Length-1 && Vector3.Distance(hero.transform.position,path.corners[corner])<.15f)corner++;
                move=path.corners[corner]-hero.transform.position;
            }
            move.y=0;move=move.magnitude>.1f?move.normalized:Vector3.zero;
            var cam=Camera.main;var f=cam.transform.forward;f.y=0;f.Normalize();var r=cam.transform.right;r.y=0;r.Normalize();
            var state=new GamepadState{leftStick=new Vector2(Vector3.Dot(move,r),Vector3.Dot(move,f)),rightStick=new Vector2(Vector3.Dot(aim,r),Vector3.Dot(aim,f)),rightTrigger=attack?1:0};
            // A draught uses a press edge. Release between attempts so low health
            // can request another available draught after the gameplay cooldown.
            if(hero.GetComponent<Combatant>().Health<hero.GetComponent<Combatant>().MaxHealth*.5f && Time.time%.5f<.1f)state=state.WithButton(GamepadButton.DpadUp);
            if(dodge)state=state.WithButton(GamepadButton.South);if(bash)state=state.WithButton(GamepadButton.LeftShoulder);if(cleave)state=state.WithButton(GamepadButton.RightShoulder);
            InputSystem.QueueStateEvent(pad,state);
            if(Time.unscaledTime>nextSample){nextSample=Time.unscaledTime+.5f;var p=hero.transform.position;report.WriteLine(string.Join(",",(Time.unscaledTime-started).ToString("F2",System.Globalization.CultureInfo.InvariantCulture),Time.frameCount,game.Stage,hero.GetComponent<Combatant>().Health.ToString(System.Globalization.CultureInfo.InvariantCulture),game.Inventory.Items.Count,p.x.ToString(System.Globalization.CultureInfo.InvariantCulture),p.z.ToString(System.Globalization.CultureInfo.InvariantCulture),(Time.unscaledDeltaTime*1000).ToString(System.Globalization.CultureInfo.InvariantCulture),UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()));report.Flush();}
        }
        private void Finish(string reason){active=false;InputSystem.QueueStateEvent(pad,new GamepadState());report?.Dispose();report=null;Debug.Log("AUDIT ended: "+reason);}
        private void OnDestroy(){report?.Dispose();if(pad!=null)InputSystem.RemoveDevice(pad);}
    }
}
#endif

