using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Oathfall.Presentation;
using Oathfall.Core;
namespace Oathfall.Editor
{
    public static class GearAssetAuthoring
    {
        public static void Build()
        {
            string folder=ExpeditionAssets.Root+"/Resources";Directory.CreateDirectory(folder+"/OathfallIcons");AssetDatabase.Refresh();
            string path=folder+"/OathfallGear.asset";var catalog=AssetDatabase.LoadAssetAtPath<GearCatalog>(path);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<GearCatalog>();AssetDatabase.CreateAsset(catalog,path);}
            var steel=ExpeditionAssets.Lit("GearSteel",new Color(.57f,.66f,.70f),.8f,.53f);
            var brass=ExpeditionAssets.Lit("GearBrass",new Color(.72f,.46f,.17f),.75f,.45f);
            var leather=ExpeditionAssets.Lit("GearLeather",new Color(.105f,.055f,.032f),0,.17f);
            var gem=ExpeditionAssets.Lit("GearMoonstone",new Color(.11f,.55f,.61f),.3f,.8f);gem.SetColor("_EmissionColor",new Color(.03f,.16f,.19f));
            steel.SetTexture("_BumpMap",null);steel.DisableKeyword("_NORMALMAP");
            foreach(var pair in new[]{(leather,"Dawnwarden_Undercloth")})
            {
                var old=AssetDatabase.LoadAssetAtPath<Material>(ExpeditionAssets.Root+"/"+pair.Item2+".mat");if(old!=null){pair.Item1.SetTexture("_BumpMap",old.GetTexture("_BumpMap"));pair.Item1.EnableKeyword("_NORMALMAP");pair.Item1.SetFloat("_BumpScale",.35f);}
            }
            for(int i=0;i<6;i++)
            {
                string slot=((ItemSlot)i).ToString();var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ExpeditionAssets.Root+"/Models/Gear"+slot+".fbx"));
                var root=new GameObject("Gear "+slot);model.transform.SetParent(root.transform,false);
                foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m.name.Contains("Brass")?brass:m.name.Contains("Leather")?leather:m.name.Contains("Gem")?gem:steel).ToArray();
                var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
                float scale=1.55f/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));model.transform.localScale*=scale;model.transform.localPosition=-bounds.center*scale;model.transform.RotateAround(Vector3.zero,Vector3.up,180);
                catalog.models[i]=PrefabUtility.SaveAsPrefabAsset(root,ExpeditionAssets.Root+"/Gear"+slot+".prefab");
                root.transform.position=Vector3.one*10000;root.transform.rotation=Quaternion.Euler(0,25,0);foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
                var camera=new GameObject("Gear icon camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=root.transform.position+new Vector3(0,.15f,-3.5f);camera.transform.LookAt(root.transform.position);camera.cullingMask=1<<30;camera.orthographic=true;camera.orthographicSize=.96f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.045f,.052f);
                var lights=new GameObject[2];for(int n=0;n<2;n++){lights[n]=new GameObject("Icon studio light",typeof(Light));lights[n].transform.position=root.transform.position+new Vector3(n==0?-1.2f:1.3f,1.2f,-1.5f);var l=lights[n].GetComponent<Light>();l.type=LightType.Point;l.range=5;l.intensity=8;l.color=n==0?new Color(1,.84f,.65f):new Color(.57f,.77f,1);l.cullingMask=1<<30;}
                var rt=new RenderTexture(256,256,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(256,256,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
                string iconPath=folder+"/OathfallIcons/"+slot+".png";File.WriteAllBytes(iconPath,image.EncodeToPNG());RenderTexture.active=previous;camera.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);Object.DestroyImmediate(camera.gameObject);Object.DestroyImmediate(root);foreach(var light in lights)Object.DestroyImmediate(light);
                AssetDatabase.ImportAsset(iconPath);var importer=(TextureImporter)AssetImporter.GetAtPath(iconPath);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.maxTextureSize=256;importer.SaveAndReimport();catalog.icons[i]=AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
            }
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
        }
    }
}

