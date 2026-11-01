using UnityEditor;
using UnityEngine;
namespace Oathfall.Editor
{
    public static class ForestDetailAuthoring
    {
        public static void BuildTrees()
        {
            for(int i=0;i<3;i++)
            {
                string name="SilverOak"+i;var root=new GameObject(name);var lods=new LOD[3];
                for(int level=0;level<3;level++)
                {
                    var source=AssetDatabase.LoadAssetAtPath<GameObject>(ExpeditionAssets.Root+"/Models/"+name+(level==0?"":"_LOD"+level)+".fbx");var model=Object.Instantiate(source,root.transform);model.name="LOD"+level;
                    lods[level]=new LOD(level==0?.18f:level==1?.075f:.012f,model.GetComponentsInChildren<Renderer>());
                }
                var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();PrefabUtility.SaveAsPrefabAsset(root,ExpeditionAssets.Root+"/Models/"+name+".prefab");Object.DestroyImmediate(root);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
