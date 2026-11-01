using System.Linq;
using UnityEngine;
using UnityEditor;
using Oathfall.Presentation;
namespace Oathfall.Editor
{
    public static class EnemyRoleAuthoring
    {
        public static void Build()
        {
            var source=AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(ExpeditionAssets.Root+"/HollowwoodRaider.asset");
            foreach(string role in new[]{"Raider","Hexblade","Breaker","Guardian"})
            {
                string name=role=="Raider"?"HollowwoodRaider":role.ToLowerInvariant();
                string model=ExpeditionAssets.Root+"/Models/Enemy"+role+".fbx";
                var importer=(ModelImporter)AssetImporter.GetAtPath(model);importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.importAnimation=false;importer.SaveAndReimport();
                var root=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(model));root.name=name;root.transform.localRotation=Quaternion.Euler(0,180,0);
                root.transform.localScale*=role=="Guardian"?1.24f:role=="Breaker"?1.10f:1;
                var avatar=AssetDatabase.LoadAllAssetsAtPath(model).OfType<Avatar>().First();if(!avatar.isValid||!avatar.isHuman)throw new System.InvalidOperationException("Invalid role avatar "+name);
                var animator=root.GetComponent<Animator>();if(animator==null)animator=root.AddComponent<Animator>();animator.avatar=avatar;animator.runtimeAnimatorController=source.AnimatorController;animator.applyRootMotion=false;
                var cloth=ExpeditionAssets.Lit(name+"_Cloth",role=="Hexblade"?new Color(.16f,.18f,.27f):role=="Guardian"?new Color(.2f,.29f,.23f):new Color(.29f,.18f,.12f),0,.16f);
                var metal=ExpeditionAssets.Lit(name+"_Metal",role=="Guardian"?new Color(.42f,.47f,.4f):new Color(.40f,.36f,.30f),role=="Guardian"?.25f:.75f,.34f);
                var trim=ExpeditionAssets.Lit(name+"_Gold",new Color(.58f,.40f,.18f),.65f,.38f);
                var dark=ExpeditionAssets.Lit(name+"_Leather",new Color(.065f,.042f,.029f),0,.12f);
                var gem=ExpeditionAssets.Lit(name+"_Rune",new Color(.25f,.66f,.58f),.25f,.7f);gem.SetColor("_EmissionColor",new Color(.1f,.32f,.23f));
                foreach(var r in root.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m.name.Contains("Gem")?gem:m.name.Contains("Brass")||m.name.Contains("Bronze")?trim:m.name.Contains("Iron")||m.name.Contains("Stone")?metal:m.name.Contains("Blackened")?dark:cloth).ToArray();
                var originals=root.GetComponentsInChildren<SkinnedMeshRenderer>();var bones=root.GetComponentsInChildren<Transform>().GroupBy(t=>t.name).ToDictionary(g=>g.Key,g=>g.First());
                var lods=new System.Collections.Generic.List<LOD>{new LOD(.12f,originals)};
                for(int level=1;level<=2;level++)
                {
                    var reduced=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ExpeditionAssets.Root+"/Models/Enemy"+role+"_LOD"+level+".fbx"),root.transform);reduced.name="LOD"+level;reduced.transform.localPosition=Vector3.zero;reduced.transform.localRotation=Quaternion.identity;reduced.transform.localScale=Vector3.one;
                    if(reduced.GetComponent<Animator>()!=null)Object.DestroyImmediate(reduced.GetComponent<Animator>());
                    var renderers=reduced.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach(var r in renderers){var original=originals.First(x=>x.name==r.name);r.bones=r.bones.Select(b=>bones[b.name]).ToArray();r.rootBone=original.rootBone;r.sharedMaterials=original.sharedMaterials;}
                    lods.Add(new LOD(level==1?.055f:.012f,renderers));
                }
                var lod=root.AddComponent<LODGroup>();lod.SetLODs(lods.ToArray());lod.RecalculateBounds();
                foreach(var socket in new[]{("WeaponSocket",HumanBodyBones.RightHand),("ShieldSocket",HumanBodyBones.LeftHand)}){var point=new GameObject(socket.Item1);point.transform.SetParent(animator.GetBoneTransform(socket.Item2),false);}
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,ExpeditionAssets.Root+"/"+name+".prefab");Object.DestroyImmediate(root);
                string path=ExpeditionAssets.Root+"/"+name+".asset";var def=AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(path);
                if(def==null){def=Object.Instantiate(source);AssetDatabase.CreateAsset(def,path);}else if(def!=source)EditorUtility.CopySerialized(source,def);
                var so=new SerializedObject(def);so.FindProperty("modelPrefab").objectReferenceValue=prefab;so.FindProperty("avatar").objectReferenceValue=avatar;so.FindProperty("stableId").stringValue="enemy.hollowwood."+role.ToLowerInvariant()+".presentation";so.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
