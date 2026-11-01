using UnityEngine;
using Oathfall.Core;
namespace Oathfall.Presentation
{
    public sealed class ActorPresentationController:MonoBehaviour,IActorView,IWeaponTrace
    {
        private ActorPresentationDefinition definition;
        private Animator animator; private LineRenderer telegraph;private PresentationAction action;
        private float flashUntil;private Renderer[] renderers;private MaterialPropertyBlock properties;
        private AudioSource voice;private float nextStep,lastStrength=-1,movement;private TrailRenderer trail;
        private WeaponMotion attackMotion;private string attackState;private float attackProgress;private bool drivenAttack;
        private Transform model;private Quaternion modelRest,moveFacing=Quaternion.identity;
        private readonly System.Collections.Generic.Dictionary<string,float> clipLengths=new System.Collections.Generic.Dictionary<string,float>();
        public ActorPresentationDefinition Definition=>definition;
        public PresentationAction Action=>action;
        public void Initialize(ActorPresentationDefinition value)
        {
            definition=value;
            if(value==null || value.ModelPrefab==null){Debug.LogError("Missing authored actor model",this);return;}
            var visual=Instantiate(value.ModelPrefab,transform);visual.name="Actor Visual";model=visual.transform;modelRest=model.localRotation;
            animator=visual.GetComponentInChildren<Animator>();
            if(animator!=null){animator.runtimeAnimatorController=value.AnimatorController;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;foreach(var clip in animator.runtimeAnimatorController.animationClips)clipLengths[clip.name.Substring(clip.name.LastIndexOf('|')+1)]=clip.length;}
            renderers=visual.GetComponentsInChildren<Renderer>();
            voice=gameObject.AddComponent<AudioSource>();voice.spatialBlend=value.Kind==ActorPresentationKind.Dawnwarden?0:.8f;voice.minDistance=4;voice.maxDistance=30;voice.rolloffMode=AudioRolloffMode.Linear;voice.volume=.35f;voice.priority=value.Kind==ActorPresentationKind.Dawnwarden?64:160;
            var hand=animator!=null&&animator.isHuman?animator.GetBoneTransform(HumanBodyBones.RightHand):null;
            if(hand!=null){var tip=new GameObject("Blade trail");tip.transform.SetParent(hand,false);tip.transform.localPosition=value.WeaponMotion!=null?value.WeaponMotion.handLocalTip:Vector3.up*.65f;trail=tip.AddComponent<TrailRenderer>();trail.sharedMaterial=value.TelegraphMaterial;trail.time=.08f;trail.startWidth=.07f;trail.endWidth=0;trail.emitting=false;trail.minVertexDistance=.06f;}
            if(value.Kind==ActorPresentationKind.HollowwoodRaider)
            {
                var line=new GameObject("Threat arc");line.transform.SetParent(transform,false);telegraph=line.AddComponent<LineRenderer>();telegraph.useWorldSpace=false;
                telegraph.sharedMaterial=value.TelegraphMaterial;telegraph.startWidth=.08f;telegraph.endWidth=.08f;telegraph.positionCount=19;
                telegraph.SetPosition(0,new Vector3(0,.06f,0));
                // Filled when an attack starts, from the same blade samples used for contact.
                for(int i=0;i<17;i++)telegraph.SetPosition(i+1,new Vector3(0,.06f,0));
                telegraph.SetPosition(18,new Vector3(0,.06f,0));line.SetActive(false);
            }
            Play("Idle",.01f);
        }
        public void PlayAttack(int combo,int ability,float duration)
        {
            action=PresentationAction.AttackWindup;if(model!=null)model.localRotation=modelRest;
            string clip=ability==1?"Shield_OneShot":ability==2?"Sword_Attack":combo==0?"Sword_A_Complete":combo==1?"Sword_B_Complete":"Sword_Regular_C";
            if(definition.Kind==ActorPresentationKind.Dawnwarden&&ability==0)clip="Dawnwarden_Combo"+(combo+1);
            if(animator!=null && clipLengths.TryGetValue(clip,out var length))animator.SetFloat("AttackRate",length/Mathf.Max(.1f,duration));
            attackState=ability==1?"Bash":ability==2?"Cleave":combo==0?"Attack":"Attack"+(combo+1);
            attackMotion=null;if(definition.WeaponMotion!=null)foreach(var motion in definition.WeaponMotion.motions)if(motion.state==attackState)attackMotion=motion;
            drivenAttack=true;attackProgress=0;Play(attackState,.045f);
        }
        public bool SampleBlade(float normalizedTime,out BladePoint grip,out BladePoint tip)
        {
            grip=tip=default;if(attackMotion==null)return false;attackMotion.Sample(normalizedTime,out var a,out var b);
            grip=new BladePoint(a.x,a.y,a.z);tip=new BladePoint(b.x,b.y,b.z);return true;
        }
        public void SetAttackProgress(float normalizedTime){attackProgress=Mathf.Clamp01(normalizedTime);}
        public void SetMovement(float speed,float localRight=0,float localForward=1)
        {
            movement=speed;if(localRight*localRight+localForward*localForward>.001f)moveFacing=Quaternion.LookRotation(new Vector3(localRight,0,localForward));
            if(animator!=null){animator.SetFloat("Move",speed,.09f,Time.deltaTime);animator.SetFloat("MoveRate",Mathf.Clamp(speed,.65f,1.35f));}
        }
        public void SetTelegraph(bool visible)
        {
            if(telegraph==null)return;
            if(visible&&attackMotion!=null)
                for(int i=0;i<17;i++){attackMotion.Sample(Mathf.Lerp(.32f,.66f,i/16f),out _,out var tip);telegraph.SetPosition(i+1,new Vector3(tip.x,.06f,tip.z));}
            telegraph.gameObject.SetActive(visible);
        }
        public void SetAction(PresentationAction next)
        {
            if(next==action)return;var previous=action;action=next;
            if(trail!=null){trail.emitting=next==PresentationAction.AttackActive;if(next==PresentationAction.Death||next==PresentationAction.Stagger){trail.emitting=false;trail.Clear();}}
            if(next==PresentationAction.AttackActive){Sound(definition.SwingSound);return;}if(next==PresentationAction.AttackRecover)return;
            if(next==PresentationAction.Dodge && model!=null)model.localRotation=moveFacing*modelRest;
            drivenAttack=false;if(animator!=null)animator.speed=1;
            string state=next==PresentationAction.Move?"Move":next==PresentationAction.AttackWindup?"Attack":next==PresentationAction.Block?"Block":next==PresentationAction.Dodge?"Dodge":next==PresentationAction.Death?"Death":next==PresentationAction.Stagger||next==PresentationAction.Hit?"Hit":"Idle";
            Play(state,next==PresentationAction.Death?.08f:.1f);
            if(next==PresentationAction.Death)SetTelegraph(false);
        }
        private void Play(string state,float blend){if(animator!=null && animator.HasState(0,Animator.StringToHash(state)))animator.CrossFadeInFixedTime(state,blend,0,0);}
        private void Sound(AudioClip clip){if(voice!=null&&clip!=null){voice.pitch=Random.Range(.93f,1.07f);voice.PlayOneShot(clip,.9f*PresentationSettings.Sfx);}}
        public void FlashHit(){Flash(Color.white);Sound(action==PresentationAction.Block?definition.BlockSound:definition.ImpactSound);}
        public void Flash(Color color,float duration=.075f){flashUntil=Time.time+duration;}
        private void LateUpdate()
        {
            if(renderers==null)return;
            if(drivenAttack && animator!=null){animator.speed=0;animator.Play(attackState,0,attackProgress);animator.Update(0);}
            if(model!=null && (action==PresentationAction.Move||action==PresentationAction.Idle||action==PresentationAction.Block))model.localRotation=Quaternion.Slerp(model.localRotation,(action==PresentationAction.Move?moveFacing:Quaternion.identity)*modelRest,1-Mathf.Exp(-Time.deltaTime*18));
            if(action==PresentationAction.Move && movement>.1f && Time.time>nextStep){nextStep=Time.time+.36f/Mathf.Clamp(movement,.65f,1.35f);Sound(definition.StepSound);}
            if(properties==null)properties=new MaterialPropertyBlock();
            float strength=Time.time<flashUntil?.35f*PresentationSettings.Flash:0;
            if(Mathf.Approximately(strength,lastStrength))return;lastStrength=strength;
            properties.SetColor("_EmissionColor",Color.white*strength);
            foreach(var renderer in renderers)if(renderer!=null)renderer.SetPropertyBlock(strength>0?properties:null);
        }
    }
}
