using System.Collections.Generic;
using Oathfall.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
namespace Oathfall.Gameplay
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController), typeof(Combatant))]
    public sealed class PlayerController : MonoBehaviour
    {
        private static readonly AttackTiming[] Combo={new AttackTiming(.22f,.22f,.24f),new AttackTiming(.18f,.22f,.28f),new AttackTiming(.26f,.26f,.32f)};
        private readonly HashSet<int> hitTargets=new HashSet<int>();
        private readonly Collider[] hits=new Collider[64];
        private IWeaponTrace weapon;
        private CharacterController controller; private Combatant combatant; private IActorView presentation;
        private float attackElapsed, bufferUntil, dodgeUntil, nextDodge, verticalSpeed, staggerUntil;
        private float bashReady, cleaveReady, dodgeProtectionUntil;
        private int comboIndex, ability;
        private Vector3 lastMove, dodgeDirection;
        public HeroId Hero {get;private set;}
        public AttackPhase AttackPhase {get;private set;}
        public HeroStats Stats {get;private set;} = HeroStats.Baseline;
        public float BashCooldown => Mathf.Max(0,bashReady-Time.time);
        public float CleaveCooldown => Mathf.Max(0,cleaveReady-Time.time);
        public bool IsDodging => Time.time < dodgeUntil;
        private void Awake() {controller=GetComponent<CharacterController>(); combatant=GetComponent<Combatant>(); presentation=GetComponent<IActorView>();weapon=GetComponent<IWeaponTrace>(); combatant.Damaged+=OnDamaged;}
        public void Initialize(HeroId hero)=>Hero=hero;
        public void SetStats(HeroStats stats) {Stats=stats;combatant.ApplyStats(stats);}
        private void OnDamaged() { if(combatant.Health<=0) CancelAttack(); }
        public void CancelAttack() {AttackPhase=AttackPhase.None; bufferUntil=0;hitTargets.Clear();}
        public void Stagger(float duration) {CancelAttack();staggerUntil=Time.time+duration;presentation?.SetAction(PresentationAction.Stagger);}
        private void Update()
        {
            var game=GameplaySession.Current;
            if(combatant.IsDead || game==null || !game.RunActive) {CancelAttack();return;}
            if(game.InputBlocked || Time.timeScale==0) return;
            var k=Keyboard.current;var pad=Gamepad.current;var mouse=Mouse.current;
            var input=Vector2.zero;
            if(k!=null) {if(k.wKey.isPressed)input.y++;if(k.sKey.isPressed)input.y--;if(k.aKey.isPressed)input.x--;if(k.dKey.isPressed)input.x++;}
            if(pad!=null) input+=pad.leftStick.ReadValue();
            var move=ToWorld(Vector2.ClampMagnitude(input,1));
            if(move.sqrMagnitude>.01f) lastMove=move.normalized;
            if(AttackPhase==AttackPhase.None && !IsDodging) Aim(move);
            bool dodge=(k?.spaceKey.wasPressedThisFrame??false)||(pad?.buttonSouth.wasPressedThisFrame??false);
            if(dodge && Time.time>=nextDodge && Time.time>=staggerUntil && AttackPhase!=AttackPhase.Active && AttackPhase!=AttackPhase.Windup)
            {
                CancelAttack();dodgeDirection=move.sqrMagnitude>.01f?move.normalized:(lastMove.sqrMagnitude>.01f?lastMove:transform.forward);
                dodgeUntil=Time.time+.42f;dodgeProtectionUntil=Time.time+.25f;nextDodge=Time.time+.85f;
                var localDodge=transform.InverseTransformDirection(dodgeDirection);presentation?.SetMovement(1,localDodge.x,localDodge.z);presentation?.SetAction(PresentationAction.Dodge);
            }
            bool block=((mouse?.rightButton.isPressed??false)||(pad?.leftTrigger.isPressed??false)) && AttackPhase==AttackPhase.None && !IsDodging && Time.time>=staggerUntil;
            combatant.IncomingDamageMultiplier=EncounterRules.GetIncomingDamageMultiplier(block,Time.time<dodgeProtectionUntil);
            bool overUi=EventSystem.current!=null && EventSystem.current.IsPointerOverGameObject();
            bool attack=(!overUi && (mouse?.leftButton.wasPressedThisFrame??false))||(pad?.rightTrigger.wasPressedThisFrame??false);
            if(!IsDodging && !block && Time.time>=staggerUntil)
            {
                if(attack) {if(AttackPhase==AttackPhase.None) StartAttack(0,0);else bufferUntil=Time.time+.12f;}
                if(AttackPhase==AttackPhase.None && ((k?.qKey.wasPressedThisFrame??false)||(pad?.leftShoulder.wasPressedThisFrame??false)) && Time.time>=bashReady) {bashReady=Time.time+5;StartAttack(0,1);}
                if(AttackPhase==AttackPhase.None && ((k?.eKey.wasPressedThisFrame??false)||(pad?.rightShoulder.wasPressedThisFrame??false)) && Time.time>=cleaveReady) {cleaveReady=Time.time+7;StartAttack(2,2);}
            }
            TickAttack(Time.deltaTime);
            if(controller.isGrounded && verticalSpeed<0)verticalSpeed=-2;
            verticalSpeed=Mathf.Max(-30,verticalSpeed-25*Time.deltaTime);
            var velocity=IsDodging?dodgeDirection*7.5f:move*Stats.MoveSpeed*(block?.4f:1)*(AttackPhase==AttackPhase.None?1:.3f);
            if(Time.time<staggerUntil)velocity=Vector3.zero;
            controller.Move((velocity+Vector3.up*verticalSpeed)*Time.deltaTime);
            if(AttackPhase==AttackPhase.None && !IsDodging && Time.time>=staggerUntil)
            {
                if(block)presentation?.SetAction(PresentationAction.Block);
                else {var local=transform.InverseTransformDirection(controller.velocity);local.y=0;presentation?.SetMovement(local.magnitude/HeroStats.Baseline.MoveSpeed,local.x,local.z);presentation?.SetAction(local.sqrMagnitude>.02f?PresentationAction.Move:PresentationAction.Idle);}
            }
        }
        private void Aim(Vector3 move)
        {
            Vector3 direction=move;
            var stick=Gamepad.current?.rightStick.ReadValue()??Vector2.zero;
            if(stick.sqrMagnitude>.1f)direction=ToWorld(stick);
            else if(Mouse.current!=null && Camera.main!=null && (Gamepad.current==null || Mouse.current.delta.ReadValue().sqrMagnitude>.1f))
            {
                var ray=Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if(new Plane(Vector3.up,transform.position).Raycast(ray,out var d)) direction=ray.GetPoint(d)-transform.position;
            }
            direction.y=0;if(direction.sqrMagnitude>.01f)transform.forward=direction.normalized;
        }
        private static Vector3 ToWorld(Vector2 input)
        {
            var camera=Camera.main;if(camera==null)return new Vector3(input.x,0,input.y);
            var f=camera.transform.forward;f.y=0;var r=camera.transform.right;r.y=0;
            return f.normalized*input.y+r.normalized*input.x;
        }
        private void StartAttack(int index,int action)
        {
            comboIndex=index;ability=action;attackElapsed=0;AttackPhase=AttackPhase.Windup;hitTargets.Clear();bufferUntil=0;
            var timing=Combo[index];presentation?.PlayAttack(index,action,timing.Windup+timing.Active+timing.Recovery);
        }
        public void TickAttack(float delta)
        {
            if(AttackPhase==AttackPhase.None || combatant.IsDead)return;
            var timing=Combo[comboIndex];var previous=attackElapsed;attackElapsed+=delta;weapon?.SetAttackProgress(attackElapsed/timing.Total);
            if(timing.CrossesActive(previous,attackElapsed))
            {
                // Subdivide the active arc even if a frame skips the entire window.
                float start=Mathf.Clamp01((previous-timing.Windup)/timing.Active),end=Mathf.Clamp01((attackElapsed-timing.Windup)/timing.Active);
                int steps=Mathf.Max(1,Mathf.CeilToInt((end-start)*40));
                for(int i=0;i<=steps;i++)Sweep(Mathf.Lerp(start,end,i/(float)steps));
            }
            var next=timing.GetPhase(attackElapsed);
            if(next==AttackPhase.None)
            {
                AttackPhase=next;
                if(bufferUntil>=Time.time && comboIndex<2 && ability==0)StartAttack(comboIndex+1,0);
                else presentation?.SetAction(PresentationAction.Idle);
                return;
            }
            if(next!=AttackPhase) {AttackPhase=next;presentation?.SetAction(next==AttackPhase.Active?PresentationAction.AttackActive:PresentationAction.AttackRecover);}
        }
        private void Sweep(float progress)
        {
            Vector3 grip,tip;
            var timing=Combo[comboIndex];
            if(ability==1){grip=transform.position+Vector3.up;tip=grip+transform.forward*1.25f;}
            else
            {
                if(weapon==null||!weapon.SampleBlade((timing.Windup+progress*timing.Active)/timing.Total,out var a,out var b))return;
                grip=transform.TransformPoint(new Vector3(a.X,a.Y,a.Z));tip=transform.TransformPoint(new Vector3(b.X,b.Y,b.Z));
            }
            int count=Physics.OverlapCapsuleNonAlloc(grip,tip,ability==1?.4f:.18f,hits,~CombatSpatial.WorldMask,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)
            {
                var target=hits[i].GetComponentInParent<Combatant>();
                if(target==null || target.Team!=CombatTeam.Enemy || target.IsDead || hitTargets.Contains(target.GetInstanceID()))continue;
                if(!CombatSpatial.Clear(transform.position+Vector3.up,target.transform.position+Vector3.up))continue;
                hitTargets.Add(target.GetInstanceID());
                target.ReceiveDamage(new DamageRequest {SourceId="hero.dawnwarden",AbilityId=ability==1?"shield-bash":ability==2?"cleaving-arc":"basic",RawAmount=Stats.Damage*(ability==1?.8f:ability==2?1.8f:1+comboIndex*.15f),ArmorPenetration=5});
                if(ability==1)target.GetComponent<EnemyBrain>()?.Stagger(1.1f);
            }
        }
    }
}
