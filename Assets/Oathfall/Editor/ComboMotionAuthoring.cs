using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Oathfall.Editor
{
    /// <summary>Three authored humanoid poses, solved to the actual sword grip and baked as muscle curves.</summary>
    public static class ComboMotionAuthoring
    {
        public static AnimationClip[] Build()
        {
            var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ExpeditionAssets.Root+"/Dawnwarden.prefab"));root.transform.position=Vector3.zero;
            var animator=root.GetComponent<Animator>();var idle=animator.runtimeAnimatorController.animationClips.First(c=>c.name.EndsWith("Sword_Idle"));
            var upper=animator.GetBoneTransform(HumanBodyBones.RightUpperArm);var lower=animator.GetBoneTransform(HumanBodyBones.RightLowerArm);var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
            var spine=animator.GetBoneTransform(HumanBodyBones.Chest);var handler=new HumanPoseHandler(animator.avatar,root.transform);var pose=new HumanPose();
            var blade=root.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name=="Forged blade");var mesh=new Mesh();blade.BakeMesh(mesh);
            var tip=mesh.vertices.OrderByDescending(v=>(blade.transform.position+blade.transform.rotation*v-hand.position).sqrMagnitude).First();var handTip=hand.InverseTransformPoint(blade.transform.position+blade.transform.rotation*tip);
            var result=new AnimationClip[3];
            try
            {
                for(int move=0;move<3;move++)
                {
                    idle.SampleAnimation(root,0);handler.GetHumanPose(ref pose);var restGrip=hand.position;var restDirection=hand.TransformVector(handTip).normalized;
                    var positions=move==0?new[]{restGrip,new Vector3(.43f,1.48f,.05f),new Vector3(.16f,1.34f,.52f),new Vector3(-.20f,1.13f,.40f),restGrip}:move==1?new[]{restGrip,new Vector3(-.20f,1.10f,.32f),new Vector3(.06f,1.32f,.55f),new Vector3(.48f,1.52f,.18f),restGrip}:new[]{restGrip,new Vector3(.27f,1.83f,.04f),new Vector3(.16f,1.48f,.53f),new Vector3(.19f,1.02f,.47f),restGrip};
                    var directions=move==0?new[]{restDirection,new Vector3(1,.2f,-.4f),new Vector3(0,0,1),new Vector3(-1,-.18f,.15f),restDirection}:move==1?new[]{restDirection,new Vector3(-1,-.15f,.05f),new Vector3(0,.18f,1),new Vector3(1,.35f,.15f),restDirection}:new[]{restDirection,new Vector3(0,1,-.25f),new Vector3(0,-.25f,1),new Vector3(0,-.68f,.73f),restDirection};
                    float duration=move==2?.84f:.68f;var curves=new AnimationCurve[HumanTrait.MuscleCount];for(int j=0;j<curves.Length;j++)curves[j]=new AnimationCurve();
                    var times=new[]{0f,.29f,.48f,.68f,1f};
                    for(int frame=0;frame<=60;frame++)
                    {
                        float t=frame/60f;int segment=0;while(segment<3&&t>times[segment+1])segment++;float f=Mathf.SmoothStep(0,1,Mathf.InverseLerp(times[segment],times[segment+1],t));
                        idle.SampleAnimation(root,0);spine.rotation=Quaternion.AngleAxis(Mathf.Sin(t*Mathf.PI*2)*(move==1?-10:10),Vector3.up)*spine.rotation;
                        var target=Vector3.Lerp(positions[segment],positions[segment+1],f);var direction=Vector3.Slerp(directions[segment].normalized,directions[segment+1].normalized,f);
                        Solve(upper,lower,hand,target);hand.rotation=Quaternion.FromToRotation(hand.TransformVector(handTip),direction)*hand.rotation;
                        handler.GetHumanPose(ref pose);for(int j=0;j<curves.Length;j++)curves[j].AddKey(t*duration,pose.muscles[j]);
                    }
                    string name="Dawnwarden_Combo"+(move+1);string path=ExpeditionAssets.Root+"/"+name+".anim";var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if(clip==null){clip=Object.Instantiate(idle);AssetDatabase.CreateAsset(clip,path);}else EditorUtility.CopySerialized(idle,clip);clip.name=name;
                    foreach(var b in AnimationUtility.GetCurveBindings(clip))
                    {
                        var old=AnimationUtility.GetEditorCurve(idle,b);AnimationUtility.SetEditorCurve(clip,b,AnimationCurve.Linear(0,old.Evaluate(0),duration,old.Evaluate(0)));
                    }
                    for(int j=0;j<curves.Length;j++)AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),HumanTrait.MuscleName[j]),curves[j]);
                    var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=false;settings.startTime=0;settings.stopTime=duration;AnimationUtility.SetAnimationClipSettings(clip,settings);clip.EnsureQuaternionContinuity();EditorUtility.SetDirty(clip);result[move]=clip;
                }
            }
            finally{handler.Dispose();Object.DestroyImmediate(mesh);Object.DestroyImmediate(root);}
            AssetDatabase.SaveAssets();return result;
        }
        private static void Solve(Transform upper,Transform lower,Transform hand,Vector3 target)
        {
            Vector3 origin=upper.position;float a=Vector3.Distance(origin,lower.position),b=Vector3.Distance(lower.position,hand.position);var delta=target-origin;float d=Mathf.Clamp(delta.magnitude,.08f,a+b-.005f);var axis=delta.normalized;
            var bend=Vector3.ProjectOnPlane(new Vector3(1,-.5f,-.2f),axis).normalized;float along=(a*a-b*b+d*d)/(2*d);var elbow=origin+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,a*a-along*along));
            upper.rotation=Quaternion.FromToRotation(lower.position-origin,elbow-origin)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
        }
    }
}
