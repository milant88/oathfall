using UnityEngine;
using System.Collections.Generic;

namespace Oathfall.Presentation
{
    /// <summary>Visibility-only cutaway. Physics and navigation never change.</summary>
    public sealed class CameraOccluder:MonoBehaviour
    {
        // Static authored foliage has visual volume beyond its narrow trunk collider.
        public bool UseVisualBounds;
        private static readonly List<CameraOccluder> canopies=new List<CameraOccluder>();
        private Bounds visualBounds;
        private Renderer[] renderers;private bool[] original;private float restoreAt;private bool concealed;
        private void Awake(){renderers=GetComponentsInChildren<Renderer>();original=new bool[renderers.Length];if(renderers.Length>0){visualBounds=renderers[0].bounds;for(int i=1;i<renderers.Length;i++)visualBounds.Encapsulate(renderers[i].bounds);}}
        private void OnEnable(){if(UseVisualBounds)canopies.Add(this);}
        public static void RevealCanopies(Vector3 camera,Vector3 target)
        {
            var direction=target-camera;float length=direction.magnitude;var ray=new Ray(camera,direction.normalized);
            for(int i=0;i<canopies.Count;i++)if(canopies[i].visualBounds.IntersectRay(ray,out float distance)&&distance<length)canopies[i].Conceal(.2f);
        }
        public void Conceal(float seconds)
        {
            if(!concealed)for(int i=0;i<renderers.Length;i++){original[i]=renderers[i].enabled;renderers[i].enabled=false;}
            concealed=true;restoreAt=Time.time+seconds;
        }
        private void LateUpdate(){if(concealed&&Time.time>=restoreAt)Restore();}
        private void Restore(){if(!concealed)return;for(int i=0;i<renderers.Length;i++)if(renderers[i]!=null)renderers[i].enabled=original[i];concealed=false;}
        private void OnDisable(){canopies.Remove(this);Restore();}
    }
}
