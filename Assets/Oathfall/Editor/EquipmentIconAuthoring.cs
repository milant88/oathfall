using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Oathfall.Editor
{
    public static class EquipmentIconAuthoring
    {
        public static void Build()
        {
            string folder=ExpeditionAssets.Root+"/Resources/OathfallIcons";Directory.CreateDirectory(folder);
            var actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ExpeditionAssets.Root+"/Dawnwarden.prefab"));
            var targets=new[]{("Weapon","Forged blade"),("Offhand","Oath kite shield"),("Head","Oath helm"),("Body","Fitted cuirass"),("Boots","Shin armour"),("Charm","Cuirass piping")};
            foreach(var item in targets)
            {
                var source=actor.GetComponentsInChildren<SkinnedMeshRenderer>().First(r=>r.name==item.Item2);
                var mesh=new Mesh();source.BakeMesh(mesh);
                var model=new GameObject("Icon subject",typeof(MeshFilter),typeof(MeshRenderer));model.layer=30;model.transform.position=Vector3.right*10000;model.GetComponent<MeshFilter>().sharedMesh=mesh;model.GetComponent<MeshRenderer>().sharedMaterials=source.sharedMaterials;
                var camera=new GameObject("Icon camera",typeof(Camera)).GetComponent<Camera>();camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.045f,.04f,1);camera.orthographic=true;
                var bounds=model.GetComponent<Renderer>().bounds;camera.transform.position=bounds.center+new Vector3(.3f,.2f,-3);camera.transform.LookAt(bounds.center);camera.orthographicSize=Mathf.Max(bounds.extents.x,bounds.extents.y)*1.25f+.04f;
                var rt=new RenderTexture(128,128,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(128,128,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,128,128),0,0);image.Apply();File.WriteAllBytes(folder+"/"+item.Item1+".png",image.EncodeToPNG());RenderTexture.active=previous;
                camera.targetTexture=null;Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(model);Object.DestroyImmediate(mesh);Object.DestroyImmediate(image);rt.Release();Object.DestroyImmediate(rt);
            }
            Object.DestroyImmediate(actor);AssetDatabase.Refresh();
            foreach(var file in Directory.GetFiles(folder,"*.png")){var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.SaveAndReimport();}
        }
    }
}
