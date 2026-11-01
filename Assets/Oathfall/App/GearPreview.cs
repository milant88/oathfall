using UnityEngine;
using UnityEngine.EventSystems;
using Oathfall.Core;
using Oathfall.Presentation;
namespace Oathfall.Gameplay
{
    /// <summary>One reusable studio; never instantiates gameplay or writes equipment state.</summary>
    public sealed class GearPreview:MonoBehaviour,IDragHandler,IScrollHandler
    {
        private GameObject studio,subject;
        private Camera previewCamera;
        private RenderTexture texture;
        private GearCatalog catalog;
        private ItemSlot shownSlot;
        private bool hasSubject;
        public ItemSlot ShownSlot=>shownSlot;
        public bool HasSubject=>subject!=null;
        public void Initialize(UnityEngine.UI.RawImage image)
        {
            catalog=GearCatalog.Load();
            studio=new GameObject("Equipment preview studio");studio.transform.position=new Vector3(10000,10000,10000);
            var cameraObject=new GameObject("Equipment preview camera",typeof(Camera));cameraObject.transform.SetParent(studio.transform,false);previewCamera=cameraObject.GetComponent<Camera>();
            previewCamera.cullingMask=1<<30;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=new Color(.025f,.035f,.043f,1);previewCamera.orthographic=true;previewCamera.orthographicSize=1.05f;previewCamera.nearClipPlane=.1f;previewCamera.farClipPlane=10;previewCamera.allowHDR=true;
            cameraObject.transform.localPosition=new Vector3(0,.16f,-3.5f);cameraObject.transform.LookAt(studio.transform.position);
            texture=new RenderTexture(768,768,24,RenderTextureFormat.ARGB32){name="Equipment preview render texture"};texture.Create();previewCamera.targetTexture=texture;image.texture=texture;
            Light("Warm key",new Vector3(-1.5f,1.7f,-1.8f),new Color(1,.83f,.65f),8);
            Light("Cool rim",new Vector3(1.6f,.8f,.5f),new Color(.48f,.72f,1),12);
            Light("Soft fill",new Vector3(.5f,-.5f,-1.8f),new Color(.73f,.83f,1),3);
            Show(ItemSlot.Weapon);studio.SetActive(gameObject.activeInHierarchy);
        }
        private void Light(string name,Vector3 position,Color color,float intensity)
        {
            var obj=new GameObject(name,typeof(Light));obj.transform.SetParent(studio.transform,false);obj.transform.localPosition=position;
            var light=obj.GetComponent<Light>();light.type=LightType.Point;light.range=5;light.intensity=intensity;light.color=color;light.cullingMask=1<<30;light.shadows=LightShadows.None;
        }
        public void Show(ItemSlot slot)
        {
            if(catalog==null||catalog.Model(slot)==null)return;
            if(hasSubject&&shownSlot==slot)return;
            if(subject!=null){subject.SetActive(false);Destroy(subject);}shownSlot=slot;hasSubject=true;
            subject=Instantiate(catalog.Model(slot),studio.transform);subject.name="Selected gear "+slot;subject.transform.localPosition=Vector3.zero;subject.transform.localRotation=Quaternion.Euler(0,22,0);
            foreach(var t in subject.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
        }
        private void Update(){if(subject!=null)subject.transform.Rotate(0,Time.unscaledDeltaTime*13,0,Space.Self);}
        public void OnDrag(PointerEventData e){if(subject!=null)subject.transform.Rotate(0,-e.delta.x*.6f,0,Space.World);}
        public void OnScroll(PointerEventData e){if(previewCamera!=null)previewCamera.orthographicSize=Mathf.Clamp(previewCamera.orthographicSize-e.scrollDelta.y*.06f,.7f,1.3f);}
        public void Rotate(float degrees){if(subject!=null)subject.transform.Rotate(0,degrees,0,Space.World);}
        private void OnEnable(){if(studio!=null)studio.SetActive(true);}
        private void OnDisable(){if(studio!=null)studio.SetActive(false);}
        private void OnDestroy(){if(previewCamera!=null)previewCamera.targetTexture=null;if(texture!=null){texture.Release();Destroy(texture);}if(studio!=null)Destroy(studio);}
    }
}

