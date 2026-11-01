using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Oathfall.Editor
{
    /// <summary>Reproducible front/three-quarter review of actual shipped actor prefabs.</summary>
    public static class ArtReviewCapture
    {
        public static void Capture()
        {
            var root=new GameObject("Temporary actor review studio");
            var origin=new Vector3(10000,10000,10000);
            root.transform.position=origin;
            RenderTexture rt=null;
            Texture2D image=null;
            var previous=RenderTexture.active;
            try
            {
                string[] names={"Dawnwarden","HollowwoodRaider","hexblade","breaker","guardian"};
                for(int i=0;i<names.Length;i++)
                {
                    var obj=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ExpeditionAssets.Root+"/"+names[i]+".prefab"),root.transform);
                    obj.transform.localPosition=new Vector3((i-2)*1.8f,0,0);
                    obj.transform.Rotate(0,20,0,Space.World);
                    var animator=obj.GetComponent<Animator>();
                    var controller=(AnimatorController)animator.runtimeAnimatorController;
                    var idle=(AnimationClip)controller.layers[0].stateMachine.states.First(s=>s.state.name=="Idle").state.motion;
                    idle.SampleAnimation(obj,.2f);
                    foreach(var t in obj.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
                    obj.GetComponent<LODGroup>().ForceLOD(0);
                }
                for(int i=0;i<2;i++)
                {
                    var light=new GameObject("Review light",typeof(Light));light.transform.SetParent(root.transform,false);light.transform.rotation=Quaternion.Euler(35,i==0?-35:150,0);
                    var l=light.GetComponent<Light>();l.type=LightType.Directional;l.color=i==0?new Color(1,.88f,.72f):new Color(.58f,.73f,1);l.intensity=i==0?2:1;l.cullingMask=1<<30;
                }
                var camera=new GameObject("Review camera",typeof(Camera)).GetComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.transform.localPosition=new Vector3(0,1.7f,-11);camera.transform.LookAt(origin+Vector3.up*1.15f);camera.cullingMask=1<<30;camera.orthographic=true;camera.orthographicSize=2.4f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.10f,.14f,.16f);
                rt=new RenderTexture(1920,960,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();
                var folder=Path.GetFullPath("docs/evidence/2026-11-01-revision5");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,"actor-lineup.png"),image.EncodeToPNG());
            }
            finally{RenderTexture.active=previous;if(rt!=null){rt.Release();Object.DestroyImmediate(rt);}if(image!=null)Object.DestroyImmediate(image);Object.DestroyImmediate(root);}
        }
    }
}
