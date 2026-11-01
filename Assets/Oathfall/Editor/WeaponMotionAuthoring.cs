using System.Linq;
using UnityEditor;
using UnityEngine;
using Oathfall.Presentation;
namespace Oathfall.Editor
{
    public static class WeaponMotionAuthoring
    {
        public static void Bake()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Bake outside Play mode");
            foreach(string name in new[]{"Dawnwarden","HollowwoodRaider","hexblade","breaker","guardian"})
            {
                var definition=AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(ExpeditionAssets.Root+"/"+name+".asset");
                var root=Object.Instantiate(definition.ModelPrefab);var animator=root.GetComponent<Animator>();
                var blade=root.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Forged blade");
                var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);var baked=new Mesh();blade.BakeMesh(baked);
                int tipIndex=Enumerable.Range(0,baked.vertexCount).OrderByDescending(i=>((blade.transform.position+blade.transform.rotation*baked.vertices[i])-hand.position).sqrMagnitude).First();
                string path=ExpeditionAssets.Root+"/"+name+"WeaponMotion.asset";
                var library=AssetDatabase.LoadAssetAtPath<WeaponMotionLibrary>(path);
                if(library==null){library=ScriptableObject.CreateInstance<WeaponMotionLibrary>();AssetDatabase.CreateAsset(library,path);}
                library.handLocalTip=hand.InverseTransformPoint((blade.transform.position+blade.transform.rotation*baked.vertices[tipIndex]));
                var states=new[]{"Attack","Attack2","Attack3","Bash","Cleave"};
                var controller=(UnityEditor.Animations.AnimatorController)animator.runtimeAnimatorController;
                library.motions=new WeaponMotion[states.Length];
                AnimationMode.StartAnimationMode();
                try
                {
                    for(int m=0;m<states.Length;m++)
                    {
                        var clip=(AnimationClip)controller.layers[0].stateMachine.states.First(s=>s.state.name==states[m]).state.motion;
                        var motion=new WeaponMotion{state=states[m],grips=new Vector3[121],tips=new Vector3[121]};
                        for(int i=0;i<121;i++)
                        {
                            AnimationMode.BeginSampling();AnimationMode.SampleAnimationClip(root,clip,clip.length*i/120f);AnimationMode.EndSampling();
                            blade.BakeMesh(baked);motion.grips[i]=hand.position-root.transform.position;
                            motion.tips[i]=(blade.transform.position+blade.transform.rotation*baked.vertices[tipIndex])-root.transform.position;
                        }
                        library.motions[m]=motion;
                    }
                }
                finally{AnimationMode.StopAnimationMode();Object.DestroyImmediate(baked);Object.DestroyImmediate(root);}
                var serialized=new SerializedObject(definition);serialized.FindProperty("weaponMotion").objectReferenceValue=library;serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(library);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
