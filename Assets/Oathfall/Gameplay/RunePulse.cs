using UnityEngine;
using Oathfall.Core;
namespace Oathfall.Gameplay
{
    public sealed class RunePulse:MonoBehaviour
    {
        public static event System.Action Fired;
        private EnemyBrain source;private Transform target;private int serial;private float started,delay,radius,damage;private LineRenderer ring;
        public static void Create(EnemyBrain source,Transform target,Vector3 point,float radius,float delay,float damage,Material material)
        {
            var obj=new GameObject("Telegraphed rune strike");obj.transform.SetParent(source.transform.parent);obj.transform.position=CombatSpatial.Ground(point)+Vector3.up*.08f;
            var pulse=obj.AddComponent<RunePulse>();pulse.source=source;pulse.serial=source.CancelSerial;pulse.target=target;pulse.radius=radius;pulse.delay=delay;pulse.damage=damage;pulse.started=Time.time;
            pulse.ring=obj.AddComponent<LineRenderer>();pulse.ring.sharedMaterial=material;pulse.ring.useWorldSpace=false;pulse.ring.loop=true;pulse.ring.positionCount=48;pulse.ring.widthMultiplier=.08f;
            for(int i=0;i<48;i++){float a=i*Mathf.PI/24;pulse.ring.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}
        }
        public bool Threatens(Vector3 position){var d=position-transform.position;d.y=0;return d.magnitude<radius+.5f&&Time.time-started>delay*.3f;}
        private void Update()
        {
            if(source==null||source.GetComponent<Combatant>().IsDead||source.CancelSerial!=serial||target==null){Destroy(gameObject);return;}
            ring.widthMultiplier=Mathf.Lerp(.05f,.18f,Mathf.Clamp01((Time.time-started)/delay));
            if(Time.time-started<delay)return;
            Fired?.Invoke();
            var delta=target.position-transform.position;delta.y=0;
            if(delta.magnitude<=radius+.25f&&CombatSpatial.Clear(transform.position+Vector3.up,target.position+Vector3.up))target.GetComponent<Combatant>().ReceiveDamage(new DamageRequest{SourceId="enemy."+source.Role,AbilityId="rune-strike",DamageType=DamageType.Arcane,RawAmount=damage});
            Destroy(gameObject);
        }
    }
}
