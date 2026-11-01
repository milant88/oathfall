using UnityEngine;

namespace Oathfall.Presentation
{
    /// <summary>Reuses one emitter per actor; damage authority stays in Gameplay.</summary>
    public sealed class CombatFeedbackView:MonoBehaviour
    {
        private LineRenderer background,healthBar;
        private ParticleSystem impacts;
        private float visibleUntil,fraction=1;
        private bool enemy;
        private readonly Vector3[] line=new Vector3[2];
        public void Initialize(Material material,Material particleMaterial,bool isEnemy)
        {
            enemy=isEnemy;
            if(enemy){background=Bar("Health backing",material,.09f,new Color(.075f,.06f,.045f));healthBar=Bar("Remaining health",material,.052f,new Color(.9f,.38f,.17f));}
            var obj=new GameObject("Reusable impact burst",typeof(ParticleSystem));obj.transform.SetParent(transform,false);obj.transform.localPosition=Vector3.up*1.05f;
            impacts=obj.GetComponent<ParticleSystem>();impacts.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=impacts.main;main.loop=false;main.playOnAwake=false;main.duration=.3f;main.maxParticles=24;main.startLifetime=new ParticleSystem.MinMaxCurve(.12f,.26f);main.startSpeed=new ParticleSystem.MinMaxCurve(1.2f,3);main.startSize=new ParticleSystem.MinMaxCurve(.025f,.065f);main.gravityModifier=.8f;main.simulationSpace=ParticleSystemSimulationSpace.World;
            main.startColor=new Color(1,.72f,.32f,.85f);var emission=impacts.emission;emission.enabled=false;
            var shape=impacts.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.09f;
            var renderer=obj.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial!=null?particleMaterial:material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        private LineRenderer Bar(string name,Material material,float width,Color color)
        {
            var obj=new GameObject(name,typeof(LineRenderer));obj.transform.SetParent(transform,false);var bar=obj.GetComponent<LineRenderer>();bar.sharedMaterial=material;bar.positionCount=2;bar.startWidth=bar.endWidth=width;bar.useWorldSpace=true;bar.enabled=false;bar.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            var properties=new MaterialPropertyBlock();properties.SetColor("_BaseColor",color);bar.SetPropertyBlock(properties);return bar;
        }
        public void ShowHit(float health,float maximum,float damage)
        {
            fraction=Mathf.Clamp01(health/Mathf.Max(1,maximum));visibleUntil=Time.time+3;
            if(impacts!=null && PresentationSettings.Flash>0)impacts.Emit(Mathf.Clamp(Mathf.RoundToInt((damage>35?14:8)*PresentationSettings.Flash),1,16));
        }
        private void LateUpdate()
        {
            if(!enemy||background==null)return;
            bool visible=fraction>0&&Time.time<visibleUntil;background.enabled=healthBar.enabled=visible;if(!visible)return;
            var cam=Camera.main;if(cam==null)return;var center=transform.position+Vector3.up*2.35f;var right=cam.transform.right*.5f;
            line[0]=center-right;line[1]=center+right;background.SetPositions(line);
            line[0]-=cam.transform.forward*.015f;line[1]=line[0]+right*2*fraction;healthBar.SetPositions(line);
        }
        private void OnDisable(){if(impacts!=null)impacts.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);if(background!=null)background.enabled=healthBar.enabled=false;}
    }
}
