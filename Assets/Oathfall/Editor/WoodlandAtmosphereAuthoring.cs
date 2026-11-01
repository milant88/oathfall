using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Oathfall.Editor
{
    public static class WoodlandAtmosphereAuthoring
    {
        public static VolumeProfile LightingProfile(string path)
        {
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="Hollowwood canopy lighting";AssetDatabase.CreateAsset(profile,path);}
            var tone=PersistentOverride<Tonemapping>(profile);tone.mode.Override(TonemappingMode.ACES);
            var color=PersistentOverride<ColorAdjustments>(profile);color.postExposure.Override(.05f);color.contrast.Override(12);color.saturation.Override(-8);
            var bloom=PersistentOverride<Bloom>(profile);bloom.intensity.Override(.16f);bloom.threshold.Override(1.15f);bloom.scatter.Override(.55f);
            var vignette=PersistentOverride<Vignette>(profile);vignette.intensity.Override(.16f);vignette.smoothness.Override(.55f);
            foreach(var component in profile.components)EditorUtility.SetDirty(component);EditorUtility.SetDirty(profile);AssetDatabase.SaveAssets();return profile;
        }
        private static T PersistentOverride<T>(VolumeProfile profile) where T:VolumeComponent
        {
            if(!profile.TryGet<T>(out var value))value=profile.Add<T>();
            if(!EditorUtility.IsPersistent(value))AssetDatabase.AddObjectToAsset(value,profile);
            value.active=true;return value;
        }
        public static void Create(Transform parent)
        {
            const string path="Assets/Oathfall/Art/AtmosphereParticle.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(material,path);}
            material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);material.SetFloat("_SrcBlend",5);material.SetFloat("_DstBlend",10);material.SetFloat("_ZWrite",0);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Oathfall/Art/SoftParticle.asset");
            if(texture==null)
            {
                texture=new Texture2D(32,32,TextureFormat.RGBA32,false);var pixels=new Color[1024];
                for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15.5f;pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),2));}
                texture.SetPixels(pixels);texture.Apply();AssetDatabase.CreateAsset(texture,"Assets/Oathfall/Art/SoftParticle.asset");
            }
            material.SetTexture("_BaseMap",texture);EditorUtility.SetDirty(material);
            foreach(var definitionPath in new[]{"Assets/Oathfall/Art/Dawnwarden.asset","Assets/Oathfall/Art/HollowwoodRaider.asset"})
            {
                var definition=AssetDatabase.LoadAssetAtPath<Oathfall.Presentation.ActorPresentationDefinition>(definitionPath);var serialized=new SerializedObject(definition);serialized.FindProperty("impactMaterial").objectReferenceValue=material;serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach(var position in new[]{new Vector3(-5,1,-30),new Vector3(18,1,1),new Vector3(-6,1,29)})
            {
                var obj=new GameObject("Drifting canopy pollen",typeof(ParticleSystem));obj.transform.SetParent(parent);obj.transform.position=position;
                var system=obj.GetComponent<ParticleSystem>();system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=system.main;main.duration=8;main.loop=true;main.prewarm=true;main.maxParticles=50;main.startLifetime=7;main.startSpeed=.06f;main.startSize=new ParticleSystem.MinMaxCurve(.025f,.065f);main.startColor=new Color(.85f,.8f,.5f,.35f);main.simulationSpace=ParticleSystemSimulationSpace.World;
                var shape=system.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(13,3,12);
                var emission=system.emission;emission.rateOverTime=5;
                var noise=system.noise;noise.enabled=true;noise.strength=.18f;noise.frequency=.25f;noise.scrollSpeed=.08f;
                var renderer=system.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            foreach(var position in new[]{new Vector3(-2.7f,1.4f,17),new Vector3(2.7f,1.4f,17),new Vector3(0,2.5f,35)})
            {
                var obj=new GameObject("Shrine amber reflection",typeof(Light));obj.transform.SetParent(parent);obj.transform.position=position;
                var light=obj.GetComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.58f,.2f);light.intensity=1.7f;light.range=6;light.shadows=LightShadows.None;
            }
        }
    }
}
