using System.Linq;
using Oathfall.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Oathfall.Editor
{
    public class ExpeditionBuildValidator:IPreprocessBuildWithReport
    {
        public int callbackOrder=>0;
        public void OnPreprocessBuild(BuildReport report)
        {
            foreach(var name in new[]{"Dawnwarden","HollowwoodRaider","hexblade","breaker","guardian"})
            {
                var definition=AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(ExpeditionAssets.Root+"/"+name+".asset");
                if(definition==null||definition.ModelPrefab==null||definition.Avatar==null||!definition.Avatar.isValid||definition.AnimatorController==null)throw new BuildFailedException("Invalid actor: "+name);
                if(definition.WeaponMotion==null||definition.WeaponMotion.motions==null||definition.WeaponMotion.motions.Length!=5||definition.WeaponMotion.motions.Any(m=>m.tips==null||m.tips.Length<120||m.grips.Length!=m.tips.Length))throw new BuildFailedException("Missing sampled weapon motion: "+name);
                var controller=definition.AnimatorController;
                if(controller.animationClips.Length<7 || controller.animationClips.Any(c=>c==null))throw new BuildFailedException("Missing animation clips: "+name);
                foreach(var r in definition.ModelPrefab.GetComponentsInChildren<Renderer>())
                    if(r.sharedMaterials.Any(m=>m==null||m.shader==null||!m.shader.isSupported))throw new BuildFailedException("Invalid material: "+r.name);
            }
            if(!EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path=="Assets/Scenes/Hollowwood.unity"))throw new BuildFailedException("Hollowwood not in build");
            var gear=GearCatalog.Load();
            if(gear==null||gear.models.Length!=6||gear.icons.Length!=6||gear.models.Any(m=>m==null)||gear.icons.Any(i=>i==null))throw new BuildFailedException("Equipment preview models/icons missing");
            var hero=AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(ExpeditionAssets.Root+"/Dawnwarden.asset");
            for(int i=1;i<=3;i++)if(!hero.AnimatorController.animationClips.Any(c=>c.name=="Dawnwarden_Combo"+i))throw new BuildFailedException("Distinct Dawnwarden combo clip missing: "+i);
            foreach(string sound in new[]{"Forest","OathTheme","Swing","Impact","Step","Loot","Equip","Rune"})if(Resources.Load<AudioClip>("OathfallAudio/"+sound)==null)throw new BuildFailedException("Required audio missing: "+sound);
            var profile=AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(ExpeditionAssets.Root+"/HollowwoodLighting.asset");
            if(profile==null||profile.components.Count<4||profile.components.Any(c=>c==null||!EditorUtility.IsPersistent(c)))throw new BuildFailedException("Hollowwood lighting overrides must be persistent sub-assets");
        }
    }
}
