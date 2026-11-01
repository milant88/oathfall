using System;
using System.IO;
using System.Linq;
using Oathfall.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
namespace Oathfall.Editor
{
    public static class ExpeditionAssets
    {
        public const string Root="Assets/Oathfall/Art";
        public const string Third="Assets/Oathfall/ThirdParty/Quaternius";
        public static void Import()
        {
            Directory.CreateDirectory(Root);AssetDatabase.Refresh();
            foreach(var path in new[]{Third+"/Characters/Superhero_Male_FullBody.fbx",Third+"/Animations/UAL1_Standard.fbx",Third+"/Animations/UAL2_Standard.fbx",Root+"/Models/Dawnwarden.fbx",Root+"/Models/HollowwoodRaider.fbx"})
            {
                var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.animationType=ModelImporterAnimationType.Human;importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;importer.importAnimation=true;
                var clips=importer.defaultClipAnimations;
                foreach(var clip in clips){clip.loopTime=clip.name.Contains("Loop")||clip.name.Contains("Idle");clip.lockRootRotation=true;clip.lockRootHeightY=true;clip.lockRootPositionXZ=true;clip.keepOriginalOrientation=true;clip.keepOriginalPositionXZ=true;}
                importer.clipAnimations=clips;importer.SaveAndReimport();
            }
            foreach(var path in AssetDatabase.FindAssets("t:Texture2D",new[]{Third,"Assets/Oathfall/ThirdParty/PolyHaven"}).Select(AssetDatabase.GUIDToAssetPath))
            {
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
                importer.maxTextureSize=2048;importer.mipmapEnabled=true;importer.streamingMipmaps=true;importer.anisoLevel=8;
                bool normal=Path.GetFileNameWithoutExtension(path).EndsWith("_Normal")||path.Contains("_nor_");bool data=normal||path.Contains("Rough")||path.Contains("rough")||path.Contains("ORM");
                importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!data;if(normal)importer.flipGreenChannel=path.Contains("_nor_dx_");importer.SaveAndReimport();
            }
            Debug.Log("Expedition sources imported");
        }
        public static Material Lit(string name,Color color,float metallic=0,float smoothness=.2f,Texture baseMap=null,Texture normal=null)
        {
            var path=Root+"/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.color=color;material.SetFloat("_Metallic",metallic);material.SetFloat("_Smoothness",smoothness);material.SetTexture("_BaseMap",baseMap);
            if(normal!=null){material.SetTexture("_BumpMap",normal);material.EnableKeyword("_NORMALMAP");}
            material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",Color.black);EditorUtility.SetDirty(material);return material;
        }
        public static void BuildActors()
        {
            
            var clips=AssetDatabase.LoadAllAssetsAtPath(Third+"/Animations/UAL1_Standard.fbx").Concat(AssetDatabase.LoadAllAssetsAtPath(Third+"/Animations/UAL2_Standard.fbx")).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
            clips=clips.Concat(new[]{CombatClipAuthoring.Join(clips.First(c=>c.name.EndsWith("|Sword_Regular_A")),clips.First(c=>c.name.EndsWith("|Sword_Regular_A_Rec")),"Sword_A_Complete"),CombatClipAuthoring.Join(clips.First(c=>c.name.EndsWith("|Sword_Regular_B")),clips.First(c=>c.name.EndsWith("|Sword_Regular_B_Rec")),"Sword_B_Complete")}).ToArray();
            clips=clips.Concat(ComboMotionAuthoring.Build()).ToArray();
            foreach(bool hero in new[]{true,false})
            {
                string name=hero?"Dawnwarden":"HollowwoodRaider";
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+name+".fbx");var root=UnityEngine.Object.Instantiate(source);root.name=name;root.transform.localRotation=Quaternion.Euler(0,180,0);
                var animator=root.GetComponent<Animator>();if(animator==null)animator=root.AddComponent<Animator>();
                var avatar=AssetDatabase.LoadAllAssetsAtPath(Root+"/Models/"+name+".fbx").OfType<Avatar>().First();
                if(!avatar.isValid||!avatar.isHuman)throw new Exception("Invalid hero avatar");animator.avatar=avatar;animator.applyRootMotion=false;
                string controllerPath=Root+"/"+name+".controller";
                var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
                if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
                var machine=controller.layers[0].stateMachine;foreach(var old in machine.states)machine.RemoveState(old.state);
                if(!controller.parameters.Any(p=>p.name=="Move"))controller.AddParameter("Move",AnimatorControllerParameterType.Float);
                if(!controller.parameters.Any(p=>p.name=="AttackRate"))controller.AddParameter("AttackRate",AnimatorControllerParameterType.Float);
                if(!controller.parameters.Any(p=>p.name=="MoveRate"))controller.AddParameter("MoveRate",AnimatorControllerParameterType.Float);
                foreach(var parameter in controller.parameters){if(parameter.name=="AttackRate"||parameter.name=="MoveRate")parameter.defaultFloat=1;}
                foreach(var obsolete in AssetDatabase.LoadAllAssetsAtPath(controllerPath).OfType<BlendTree>().ToArray())UnityEngine.Object.DestroyImmediate(obsolete,true);
                var bindings=new[]{("Idle","Sword_Idle"),("Move","Jog_Fwd_Loop"),("Attack",hero?"Dawnwarden_Combo1":"Sword_A_Complete"),("Attack2",hero?"Dawnwarden_Combo2":"Sword_B_Complete"),("Attack3",hero?"Dawnwarden_Combo3":"Sword_Regular_C"),("Bash","Shield_OneShot"),("Cleave","Sword_Attack"),("Block","Idle_Shield_Loop"),("Dodge","Roll"),("Hit","Hit_Chest"),("Death","Death01")};
                foreach(var pair in bindings)
                {
                    var clip=clips.First(c=>c.name.EndsWith(pair.Item2));var state=machine.AddState(pair.Item1);state.motion=clip;
                    if(pair.Item1.StartsWith("Attack")||pair.Item1=="Bash"||pair.Item1=="Cleave"){state.speedParameter="AttackRate";state.speedParameterActive=true;}
                    if(pair.Item1=="Move")
                    {
                        var tree=new BlendTree{name="Grounded locomotion",blendType=BlendTreeType.Simple1D,blendParameter="Move",useAutomaticThresholds=false};AssetDatabase.AddObjectToAsset(tree,controller);
                        tree.AddChild(clips.First(c=>c.name.EndsWith("Sword_Idle")),0);tree.AddChild(clips.First(c=>c.name.EndsWith("Walk_Loop")),.45f);tree.AddChild(clip,1);state.motion=tree;state.speedParameter="MoveRate";state.speedParameterActive=true;
                    }
                    if(pair.Item1=="Dodge")state.speed=clip.length/.42f;if(pair.Item1=="Idle")machine.defaultState=state;
                }
                animator.runtimeAnimatorController=controller;
                var leather=Surface(name+"_Undercloth","brown_leather",new Color(.65f,.55f,.45f),0);
                var cloth=Surface(name+"_Cloth","fabric_pattern_07",hero?new Color(.35f,.45f,.32f):new Color(.58f,.3f,.2f),0);
                var steel=Surface(name+"_Steel","metal_plate",hero?new Color(.6f,.66f,.68f):new Color(.4f,.3f,.22f),.8f);
                // Scanned dark plate albedo erased the actor silhouette at gameplay scale.
                // Keep the scanned normals and roughness with an authored steel albedo.
                steel.SetTexture("_BaseMap",null);steel.color=hero?new Color(.5f,.57f,.62f):new Color(.38f,.33f,.27f);steel.SetFloat("_Smoothness",.65f);
                var gold=Lit(name+"_Trim",hero?new Color(.62f,.4f,.14f):new Color(.27f,.16f,.09f),.65f,.45f);
                var skin=Lit(name+"_Skin",new Color(.4f,.27f,.18f),0,.25f);
                foreach(var r in root.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>m.name.Contains("Steel")||m.name.Contains("Iron")?steel:m.name.Contains("Brass")||m.name.Contains("Bronze")?gold:m.name=="Skin"?skin:m.name.Contains("Cloth")?cloth:leather).ToArray();
                var originals=root.GetComponentsInChildren<SkinnedMeshRenderer>();
                var bones=root.GetComponentsInChildren<Transform>().GroupBy(t=>t.name).ToDictionary(g=>g.Key,g=>g.First());
                var lods=new System.Collections.Generic.List<LOD>{new LOD(.12f,originals)};
                for(int level=1;level<=2;level++)
                {
                    var reduced=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+name+"_LOD"+level+".fbx"),root.transform);
                    reduced.name="LOD"+level;reduced.transform.localPosition=Vector3.zero;reduced.transform.localRotation=Quaternion.identity;reduced.transform.localScale=Vector3.one;
                    var duplicateAnimator=reduced.GetComponent<Animator>();if(duplicateAnimator!=null)UnityEngine.Object.DestroyImmediate(duplicateAnimator);
                    var renderers=reduced.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach(var renderer in renderers){var original=originals.FirstOrDefault(r=>r.name==renderer.name);if(original==null)throw new Exception("Missing LOD renderer "+renderer.name);renderer.bones=renderer.bones.Select(b=>bones[b.name]).ToArray();renderer.rootBone=original.rootBone;renderer.sharedMaterials=original.sharedMaterials;}
                    lods.Add(new LOD(level==1?.055f:.012f,renderers));
                }
                var lod=root.AddComponent<LODGroup>();lod.SetLODs(lods.ToArray());lod.RecalculateBounds();
                foreach(var socket in new[]{("WeaponSocket",HumanBodyBones.RightHand),("ShieldSocket",HumanBodyBones.LeftHand)}){var point=new GameObject(socket.Item1);point.transform.SetParent(animator.GetBoneTransform(socket.Item2),false);}
                var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root);
                var definition=AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(Root+"/"+name+".asset");
                if(definition==null){definition=ScriptableObject.CreateInstance<ActorPresentationDefinition>();AssetDatabase.CreateAsset(definition,Root+"/"+name+".asset");}
                var so=new SerializedObject(definition);so.FindProperty("modelPrefab").objectReferenceValue=prefab;so.FindProperty("avatar").objectReferenceValue=avatar;so.FindProperty("animatorController").objectReferenceValue=controller;so.ApplyModifiedPropertiesWithoutUndo();
                foreach(var sound in new[]{("swingSound","SwordSwing"),("impactSound","SteelImpact"),("stepSound","Footstep"),("blockSound","ShieldBlock")})so.FindProperty(sound.Item1).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"/Audio/"+sound.Item2+".wav");
                so.FindProperty("kind").enumValueIndex=hero?0:1;so.FindProperty("stableId").stringValue=hero?"hero.dawnwarden.presentation":"enemy.hollowwood.raider.presentation";
                var threat=Lit("ThreatAmber",new Color(1,.32f,.06f));threat.shader=Shader.Find("Universal Render Pipeline/Unlit");EditorUtility.SetDirty(threat);so.FindProperty("telegraphMaterial").objectReferenceValue=threat;so.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();Debug.Log("Rigged actor prefabs and animation bindings created");
        }
        private static Material Surface(string name,string source,Color tint,float metallic)
        {
            string directory="Assets/Oathfall/ThirdParty/PolyHaven/Surfaces/"+source;
            var paths=Directory.GetFiles(directory).Where(p=>p.EndsWith(".jpg")).Select(p=>p.Replace('\\','/')).ToArray();
            var color=AssetDatabase.LoadAssetAtPath<Texture2D>(paths.First(p=>p.Contains("diff")||p.Contains("Diff")||p.Contains("albedo")||p.Contains("_col_")));
            var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(paths.First(p=>p.Contains("nor_gl")));
            var material=Lit(name,tint,metallic,.3f,color,normal);
            string roughPath=paths.First(p=>p.ToLowerInvariant().Contains("rough"));var importer=(TextureImporter)AssetImporter.GetAtPath(roughPath);
            importer.isReadable=true;importer.sRGBTexture=false;importer.SaveAndReimport();var rough=AssetDatabase.LoadAssetAtPath<Texture2D>(roughPath);
            var pixels=rough.GetPixels();for(int i=0;i<pixels.Length;i++)pixels[i]=new Color(metallic,0,0,1-pixels[i].r);
            var packed=new Texture2D(rough.width,rough.height,TextureFormat.RGBA32,true,true);packed.SetPixels(pixels);packed.Apply();
            string packedPath=Root+"/"+name+"_MetalSmooth.png";File.WriteAllBytes(packedPath,packed.EncodeToPNG());UnityEngine.Object.DestroyImmediate(packed);AssetDatabase.ImportAsset(packedPath);
            var packedImporter=(TextureImporter)AssetImporter.GetAtPath(packedPath);packedImporter.sRGBTexture=false;packedImporter.maxTextureSize=2048;packedImporter.SaveAndReimport();
            material.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath));material.EnableKeyword("_METALLICSPECGLOSSMAP");material.SetFloat("_Smoothness",1);
            importer.isReadable=false;importer.SaveAndReimport();return material;
        }
        private static void AddPlate(GameObject root,string name,string bone,Vector3 offset,Vector3 scale,Material material)
        {
            var parent=root.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name==bone);if(parent==null)return;
            var obj=MeshObject(name,PlateMesh(scale),material);obj.transform.SetParent(parent,false);obj.transform.localPosition=offset;
        }
        public static GameObject MeshObject(string name,Mesh mesh,Material material)
        {
            string path=Root+"/"+name.Replace(" ","_")+"_"+mesh.vertexCount+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing==null){AssetDatabase.CreateAsset(mesh,path);existing=mesh;}
            var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.GetComponent<MeshFilter>().sharedMesh=existing;obj.GetComponent<MeshRenderer>().sharedMaterial=material;return obj;
        }
        public static Mesh PlateMesh(Vector3 radii)
        {
            const int sides=12,rings=7;var vertices=new Vector3[sides*rings];var uv=new Vector2[vertices.Length];var indices=new System.Collections.Generic.List<int>();
            for(int y=0;y<rings;y++)for(int x=0;x<sides;x++)
            {
                float latitude=Mathf.Lerp(-Mathf.PI*.49f,Mathf.PI*.49f,y/(float)(rings-1)),longitude=x*Mathf.PI*2/sides;int i=y*sides+x;
                vertices[i]=Vector3.Scale(new Vector3(Mathf.Cos(longitude)*Mathf.Cos(latitude),Mathf.Sin(latitude),Mathf.Sin(longitude)*Mathf.Cos(latitude)),radii);uv[i]=new Vector2(x/(float)sides,y/(float)(rings-1));
                if(y<rings-1){int next=y*sides+(x+1)%sides;indices.AddRange(new[]{i,i+sides,next,next,i+sides,next+sides});}
            }
            var mesh=new Mesh();mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=indices.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        private static Mesh SwordMesh()
        {
            var v=new[]{new Vector3(-.045f,0,0),new Vector3(.045f,0,0),new Vector3(-.06f,.85f,0),new Vector3(.06f,.85f,0),new Vector3(0,1.15f,0),new Vector3(0,.5f,.025f),new Vector3(0,.5f,-.025f)};
            var t=new[]{0,5,1,0,2,5,2,4,5,4,3,5,3,1,5,0,1,6,0,6,2,2,6,4,4,6,3,3,6,1};var mesh=new Mesh{vertices=v,triangles=t};mesh.RecalculateNormals();return mesh;
        }
    }
}


