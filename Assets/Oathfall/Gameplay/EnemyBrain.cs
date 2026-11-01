using Oathfall.Core;
using UnityEngine;
using UnityEngine.AI;
namespace Oathfall.Gameplay
{
    [RequireComponent(typeof(Combatant),typeof(NavMeshAgent))]
    public sealed class EnemyBrain:MonoBehaviour
    {
        private Combatant health;private IActorView visual;private IWeaponTrace weapon;private NavMeshAgent agent;private Transform target;
        private float nextAttack,attackStarted,recoveryEnd;private bool attacking,hit;private int index,attackNumber;private Material threatMaterial;
        public EnemyRole Role{get;private set;}public int CancelSerial{get;private set;}
        private void Awake(){health=GetComponent<Combatant>();visual=GetComponent<IActorView>();weapon=GetComponent<IWeaponTrace>();agent=GetComponent<NavMeshAgent>();}
        public void Initialize(Transform hero,int encounterIndex,EnemyRole role=EnemyRole.Raider,Material material=null)
        {
            target=hero;index=encounterIndex;Role=role;threatMaterial=material;nextAttack=Time.time+1+index*.35f;
            agent.speed=role==EnemyRole.Breaker?2.2f:2.7f;agent.acceleration=12;agent.angularSpeed=520;agent.radius=.4f;agent.height=2;agent.stoppingDistance=1.1f;agent.avoidancePriority=30+index%40;
        }
        public void Stagger(float duration){if(health.IsDead)return;CancelSerial++;attacking=false;recoveryEnd=Time.time+duration;nextAttack=recoveryEnd+.4f;visual?.SetTelegraph(false);visual?.SetAction(PresentationAction.Stagger);if(agent.isOnNavMesh)agent.isStopped=true;}
        private void Update()
        {
            if(target==null||health.IsDead||target.GetComponent<Combatant>().IsDead||!agent.isOnNavMesh||Time.timeScale==0)return;
            if(Time.time<recoveryEnd)return;
            if(attacking)
            {
                float previous=(Time.time-Time.deltaTime-attackStarted)/1.35f,current=(Time.time-attackStarted)/1.35f;weapon?.SetAttackProgress(current);
                if(current>=.32f&&previous<.66f)
                {
                    visual?.SetTelegraph(false);visual?.SetAction(PresentationAction.AttackActive);
                    for(int i=0;i<=8&&!hit;i++)Strike(Mathf.Lerp(Mathf.Max(.32f,previous),Mathf.Min(.66f,current),i/8f));
                }
                if(current>=.66f)visual?.SetAction(PresentationAction.AttackRecover);
                if(current>=1){attacking=false;visual?.SetAction(PresentationAction.Idle);}
                return;
            }
            var delta=target.position-transform.position;delta.y=0;float range=Role==EnemyRole.Hexblade?7:Role==EnemyRole.Guardian?3:Role==EnemyRole.Breaker?2.2f:1.35f;
            if(delta.magnitude>range){agent.isStopped=false;agent.SetDestination(target.position);visual?.SetAction(PresentationAction.Move);visual?.SetMovement(agent.velocity.magnitude/HeroStats.Baseline.MoveSpeed);return;}
            agent.isStopped=true;visual?.SetAction(PresentationAction.Idle);if(delta.sqrMagnitude>.01f)transform.forward=delta.normalized;
            if(Time.time<nextAttack)return;
            nextAttack=Time.time+(Role==EnemyRole.Hexblade?3.8f:Role==EnemyRole.Guardian?3.4f:2.5f)+(index%3)*.13f;attackNumber++;
            attackStarted=Time.time;attacking=true;hit=false;visual?.PlayAttack(0,0,1.35f);visual?.SetTelegraph(Role==EnemyRole.Raider);
            if(Role!=EnemyRole.Raider)
            {
                hit=true;var point=Role==EnemyRole.Breaker?transform.position+transform.forward:target.position;
                RunePulse.Create(this,target,point,Role==EnemyRole.Guardian?2.6f:Role==EnemyRole.Breaker?2.2f:1.55f,Role==EnemyRole.Guardian?1.35f:1.1f,Role==EnemyRole.Hexblade?10:22,threatMaterial);
                if(Role==EnemyRole.Guardian&&health.Health<health.MaxHealth*.55f)
                {
                    RunePulse.Create(this,target,target.position+transform.right*3.5f,1.6f,1.8f,16,threatMaterial);
                    RunePulse.Create(this,target,target.position-transform.right*3.5f,1.6f,1.8f,16,threatMaterial);
                }
            }
        }
        private void Strike(float progress)
        {
            if(weapon==null||!weapon.SampleBlade(progress,out var a,out var b))return;
            var grip=transform.TransformPoint(new Vector3(a.X,a.Y,a.Z));var tip=transform.TransformPoint(new Vector3(b.X,b.Y,b.Z));
            var shape=target.GetComponent<CharacterController>();var closest=shape.ClosestPoint(Vector3.Lerp(grip,tip,.65f));
            var edge=tip-grip;float t=Mathf.Clamp01(Vector3.Dot(closest-grip,edge)/Mathf.Max(.001f,edge.sqrMagnitude));
            if(Vector3.Distance(closest,grip+edge*t)>.28f||!CombatSpatial.Clear(transform.position+Vector3.up,target.position+Vector3.up))return;
            hit=true;target.GetComponent<Combatant>().ReceiveDamage(new DamageRequest{SourceId="enemy.raider",AbilityId="blade",RawAmount=12});
        }
    }
}
