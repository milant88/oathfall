using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Oathfall.Editor
{
    public static class CombatClipAuthoring
    {
        public static AnimationClip Join(AnimationClip attack,AnimationClip recovery,string name)
        {
            string path=ExpeditionAssets.Root+"/"+name+".anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if(clip==null){clip=Object.Instantiate(attack);AssetDatabase.CreateAsset(clip,path);}else EditorUtility.CopySerialized(attack,clip);
            clip.name=name;float offset=attack.length;
            foreach(var binding in AnimationUtility.GetCurveBindings(attack).Union(AnimationUtility.GetCurveBindings(recovery)))
            {
                var first=AnimationUtility.GetEditorCurve(attack,binding);var last=AnimationUtility.GetEditorCurve(recovery,binding);
                if(first==null||last==null)continue;
                var keys=first.keys.Where(k=>k.time<offset-.0001f).Concat(last.keys.Select(k=>{k.time+=offset;return k;})).ToArray();
                AnimationUtility.SetEditorCurve(clip,binding,new AnimationCurve(keys));
            }
            clip.EnsureQuaternionContinuity();EditorUtility.SetDirty(clip);return clip;
        }
    }
}
