using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Oathfall.Gameplay
{
    public sealed class TopDownCamera : MonoBehaviour
    {
        public Transform Target { get; set; }
        private float shake;
        private float zoom=1,desiredZoom=1;
        private readonly RaycastHit[] occlusionHits=new RaycastHit[64];
        public float Zoom=>zoom;
        public void Impulse()=>shake=.16f*Oathfall.Presentation.PresentationSettings.Shake;
        public void SetPreview(Vector3 position) { transform.position=position+new Vector3(0,14,-12); transform.rotation=Quaternion.Euler(49,0,0); }

        public static TopDownCamera Create()
        {
            var existing = Camera.main;
            var cameraObject = existing != null ? existing.gameObject : new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.06f, 0.055f);
            camera.fieldOfView = 45f;
            return cameraObject.GetComponent<TopDownCamera>() ?? cameraObject.AddComponent<TopDownCamera>();
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            if(Time.timeScale>0 && !(GameplaySession.Current?.InputBlocked??false) && !(EventSystem.current?.IsPointerOverGameObject()??false))
            {
                float wheel=Mouse.current?.scroll.ReadValue().y??0;
                if(Mathf.Abs(wheel)>.01f)desiredZoom=Mathf.Clamp(desiredZoom-Mathf.Sign(wheel)*.07f,.84f,1.16f);
            }
            float blend=1-Mathf.Exp(-Time.deltaTime*10);zoom=Mathf.Lerp(zoom,desiredZoom,blend);
            var desired = Target.position + new Vector3(0f, 14f, -12f)*zoom;
            transform.position = Vector3.Lerp(transform.position, desired, blend);
            if(shake>.001f){transform.position+=new Vector3(Mathf.Sin(Time.time*93),Mathf.Cos(Time.time*77),0)*shake;shake=Mathf.MoveTowards(shake,0,Time.deltaTime);}
            transform.rotation = Quaternion.Euler(49f,0f,0f);
            // Include feet/ground telegraphs: a clear ray to the head can still hide the body.
            var sight=Target.position+Vector3.up*.35f-transform.position;
            int count=Physics.SphereCastNonAlloc(transform.position,.22f,sight.normalized,occlusionHits,sight.magnitude,CombatSpatial.WorldMask,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)occlusionHits[i].collider.GetComponentInParent<Oathfall.Presentation.CameraOccluder>()?.Conceal(.2f);
            Oathfall.Presentation.CameraOccluder.RevealCanopies(transform.position,Target.position+Vector3.up*.35f);
        }
    }
}
