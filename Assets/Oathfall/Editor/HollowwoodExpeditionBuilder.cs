using System;
using System.IO;
using System.Linq;
using Oathfall.Presentation;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Oathfall.Editor
{
    public static class HollowwoodExpeditionBuilder
    {
        private static Transform root;private static Terrain terrain;
        private static readonly System.Collections.Generic.Dictionary<string,Material> materials=new System.Collections.Generic.Dictionary<string,Material>();
        private const string Art=ExpeditionAssets.Root;
        private static float PathX(float z)
        {
            if(z<=29)return 12*Mathf.Sin(Mathf.Clamp01((z+28)/57)*Mathf.PI);
            var stops=Oathfall.Core.ExpeditionContent.Stops;
            for(int i=3;i<stops.Length;i++)if(z<=stops[i].Z)return Mathf.Lerp(stops[i-1].X,stops[i].X,Mathf.InverseLerp(stops[i-1].Z,stops[i].Z,z));
            return 0;
        }
        private static float ClearingDistance(float x,float z)=>Oathfall.Core.ExpeditionContent.Stops.Min(s=>Vector2.Distance(new Vector2(x,z),new Vector2(s.X,s.Z)));
        private static float SideDistance(float x,float z)
        {
            var point=new Vector2(x,z);var a=new Vector2(12,-5);var b=new Vector2(-12,10);var c=new Vector2(-12,23);var d=new Vector2(0,29);
            return Mathf.Min(Segment(point,a,b),Mathf.Min(Segment(point,b,c),Segment(point,c,d)));
        }
        private static float Segment(Vector2 p,Vector2 a,Vector2 b)=>Vector2.Distance(p,a+(b-a)*Mathf.Clamp01(Vector2.Dot(p-a,b-a)/(b-a).sqrMagnitude));
        public static void Build()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play before authoring");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            root=new GameObject("Hollowwood — Fallen Oath Expedition").transform;materials.Clear();
            var data=new TerrainData {heightmapResolution=513,size=new Vector3(110,12,570),alphamapResolution=512};
            float[,] heights=new float[513,513];
            for(int z=0;z<513;z++)for(int x=0;x<513;x++)
            {
                float wx=x/512f*110-55,wz=z/512f*570-65,d=Mathf.Min(Mathf.Abs(wx-PathX(wz)),SideDistance(wx,wz));
                float edge=Mathf.SmoothStep(0,1,Mathf.Clamp01((d-10)/18));
                heights[z,x]=(2+edge*(2.5f*Mathf.PerlinNoise(wx*.07f+40,wz*.06f+40)+Mathf.Max(0,d-30)*.16f))/12;
            }
            data.SetHeights(0,0,heights);
            data=Save(data,Art+"/HollowwoodTerrain.asset");
            var groundLayer=new TerrainLayer {diffuseTexture=Texture("Assets/Oathfall/ThirdParty/PolyHaven/ForestGround04_2K","diff"),normalMapTexture=Texture("Assets/Oathfall/ThirdParty/PolyHaven/ForestGround04_2K","nor_gl"),tileSize=new Vector2(4,4),smoothness=.1f};
            var stoneLayer=new TerrainLayer {diffuseTexture=Texture("Assets/Oathfall/ThirdParty/PolyHaven/Cobblestone01_2K","diff"),normalMapTexture=Texture("Assets/Oathfall/ThirdParty/PolyHaven/Cobblestone01_2K","nor"),tileSize=new Vector2(4,4),smoothness=.18f};
            groundLayer=Save(groundLayer,Art+"/ForestFloor.terrainlayer");stoneLayer=Save(stoneLayer,Art+"/OldRoad.terrainlayer");var mossLayer=new TerrainLayer{diffuseTexture=Texture("Assets/Oathfall/ThirdParty/PolyHaven/LeafyGrass_2K","diff"),normalMapTexture=Texture("Assets/Oathfall/ThirdParty/PolyHaven/LeafyGrass_2K","nor_gl"),tileSize=new Vector2(3,3),smoothness=.12f};mossLayer=Save(mossLayer,Art+"/WoodlandUnderstory.terrainlayer");data.terrainLayers=new[]{groundLayer,stoneLayer,mossLayer};
            float[,,] alpha=new float[512,512,3];
            for(int z=0;z<512;z++)for(int x=0;x<512;x++)
            {
                float wx=x/511f*110-55,wz=z/511f*570-65;float d=Mathf.Abs(wx-PathX(wz));
                float shrine=ClearingDistance(wx,wz);
                float road=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.2f,3.5f,d+(Mathf.PerlinNoise(wx*.3f+60,wz*.3f+60)-.5f)));
                float paving=(wz>150&&wz<240)?.22f:.62f+.18f*Mathf.PerlinNoise(wx*.55f+100,wz*.52f);road=Mathf.Max(road,(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(6,10,shrine)))*paving);alpha[z,x,1]=road;alpha[z,x,0]=1-road;
                road=Mathf.Max(road,(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.6f,1.5f,SideDistance(wx,wz))))*.6f);float moss=Mathf.Clamp01((d-2)/5)*(.55f+.4f*Mathf.PerlinNoise(wx*.18f+70,wz*.18f+70));alpha[z,x,1]=road;alpha[z,x,2]=(1-road)*moss;alpha[z,x,0]=(1-road)*(1-moss);
            }
            var dryMask=new Texture2D(4,4,TextureFormat.RGBA32,false,true);dryMask.name="Dry forest surface mask";dryMask.SetPixels(Enumerable.Repeat(new Color(0,1,.5f,.09f),16).ToArray());dryMask.Apply();dryMask=Save(dryMask,Art+"/DryForestMask.asset");
            foreach(var layer in new[]{groundLayer,stoneLayer,mossLayer}){layer.maskMapTexture=dryMask;layer.maskMapRemapMin=Vector4.zero;layer.maskMapRemapMax=Vector4.one;layer.normalScale=.7f;EditorUtility.SetDirty(layer);}
            data.SetAlphamaps(0,0,alpha);
            var land=Terrain.CreateTerrainGameObject(data);land.name="Hollowwood forest ground";land.transform.SetParent(root);land.transform.position=new Vector3(-55,-2,-65);land.layer=8;terrain=land.GetComponent<Terrain>();
            var terrainMat=new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));terrainMat=Save(terrainMat,Art+"/ForestTerrain.mat");terrain.materialTemplate=terrainMat;terrain.drawInstanced=true;
            var rng=new System.Random(8142);
            for(int i=0;i<1550;i++)
            {
                float z=-52+(float)rng.NextDouble()*540,x=-38+(float)rng.NextDouble()*76;
                if(Mathf.Abs(x-PathX(z))<7.5f || SideDistance(x,z)<3.8f || ClearingDistance(x,z)<12)continue;
                var tree=Place("@SilverOak"+(i%3)+".prefab",new Vector3(x,0,z),7+(float)rng.NextDouble()*4,(float)rng.NextDouble()*360,false);tree.AddComponent<CameraOccluder>().UseVisualBounds=true;var trunk=new GameObject("Trunk collision");trunk.transform.SetParent(root);trunk.transform.position=new Vector3(x,terrain.SampleHeight(new Vector3(x,0,z))+terrain.transform.position.y,z);trunk.layer=8;var trunkCollider=trunk.AddComponent<CapsuleCollider>();trunkCollider.center=Vector3.up*2;trunkCollider.height=4;trunkCollider.radius=.4f;
            }
            for(int i=0;i<5750;i++)
            {
                float z=-44+(float)rng.NextDouble()*530,x=PathX(z)-15+(float)rng.NextDouble()*30;
                float distance=Mathf.Abs(x-PathX(z));
                if(distance<2.8f || SideDistance(x,z)<1.7f || Vector2.Distance(new Vector2(x,z),new Vector2(0,29))<9)continue;
                // Quiet patches and dense banks give the path a readable silhouette.
                if(Mathf.PerlinNoise(x*.17f+20,z*.13f+20)<.46f)continue;
                Place(i%3==0?"@WoodlandFern"+(i/3%3)+".fbx":"@WoodlandGrass.fbx",new Vector3(x,0,z),i%3==0?.3f+(float)rng.NextDouble()*.2f:.22f+(float)rng.NextDouble()*.22f,(float)rng.NextDouble()*360,false);
            }
            for(int i=0;i<260;i++)
            {
                float z=-47+(float)rng.NextDouble()*530,x=PathX(z)+(i%2==0?-1:1)*(6.5f+(float)rng.NextDouble()*6);
                if(SideDistance(x,z)<3 || Vector2.Distance(new Vector2(x,z),new Vector2(0,29))<11)continue;
                Place("@WoodlandBoulder"+(i%3)+".fbx",new Vector3(x,0,z),.7f+(float)rng.NextDouble()*1.7f,(float)rng.NextDouble()*360,true);
                // Removed angular root spikes; use grounded rock/fern transitions.
                Place("@WoodlandFern"+(i%3)+".fbx",new Vector3(x-1.2f,0,z-.7f),.45f,i*73,false);
            }
            // Deliberate banks and wayside objects; preserve the road and combat-centre clearance.
            for(int i=0;i<520;i++)
            {
                float z=-40+(float)rng.NextDouble()*520,x=PathX(z)+(i%2==0?-1:1)*(4.2f+(float)rng.NextDouble()*8);
                if(SideDistance(x,z)<2||ClearingDistance(x,z)<10)continue;
                Place(i%4==0?"@WaysideFlowers.fbx":"@HazelShrub.fbx",new Vector3(x,0,z),i%4==0?.35f:.65f+(float)rng.NextDouble()*.5f,i*71,false);
            }
            foreach(var stop in Oathfall.Core.ExpeditionContent.Stops)
            {
                // Each stop gains a small readable story cluster on its outer bank.
                for(int d=0;d<5;d++)Place("Village/Prop_Brick"+(1+d%4)+".fbx",new Vector3(stop.X-9+d*.65f,0,stop.Z+7),.23f+d*.035f,d*37,false);
                Place("@HazelShrub.fbx",new Vector3(stop.X-9,0,stop.Z+8),1.1f,70,false);
                Place("@WaysideFlowers.fbx",new Vector3(stop.X+8,0,stop.Z-7),.5f,35,false);
            }
            // Authored landmarks: entrance, destroyed roadside camp, shrine wall and waystone.
            Place("@ShrineStoneArch.fbx",new Vector3(0,0,-36),5,0,true);
            Place("Village/Prop_Wagon.fbx",new Vector3(18,0,-3),2.2f,-35,true);
            Place("Village/Prop_Wagon.fbx",new Vector3(6,0,5),1.8f,120,true);
            for(int i=0;i<6;i++)Place("Village/Prop_Crate.fbx",new Vector3(18+i%3*1.1f,0,2+i/3*1.1f),.85f,i*17,true);
            for(int i=0;i<7;i++)if(i<2 || i>4)Place("Village/Prop_WoodenFence_Single.fbx",new Vector3(3+i*3,0,-9),1.2f,8,true);
            for(int i=0;i<9;i++)
            {
                if(i==4)continue;float a=i/8f*Mathf.PI;var p=new Vector3(Mathf.Cos(a)*11,0,29+Mathf.Sin(a)*11);
                Place("@ShrineBrokenWall.fbx",p,2.4f,90-i*22.5f,true);
            }
            Place("@ShrineStoneArch.fbx",new Vector3(0,0,17),4.5f,0,true);
            for(int i=0;i<30;i++)
            {
                float z=-32+i*2.3f,x=PathX(z)+(i%2==0?-1:1)*(4+i%4);
                Place("Village/Prop_Brick"+(1+i%4)+".fbx",new Vector3(x,0,z),.18f+(i%3)*.1f,i*47,false);
            }
            Place("Nature/DeadTree_1.fbx",new Vector3(-8,0,23),5,35,true);
            Place("Nature/DeadTree_2.fbx",new Vector3(11,0,32),5,80,true);
            var statue=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"/Dawnwarden.prefab"),root);statue.name="Broken Oathkeeper Monument";statue.transform.position=new Vector3(0,0,36);statue.transform.localScale=Vector3.one*2.2f;foreach(var reduced in statue.GetComponentsInChildren<Transform>().Where(t=>t.name=="LOD1"||t.name=="LOD2").ToArray())UnityEngine.Object.DestroyImmediate(reduced.gameObject);var statueLod=statue.GetComponent<LODGroup>();if(statueLod!=null)UnityEngine.Object.DestroyImmediate(statueLod);
            var stone=ExpeditionAssets.Lit("MonumentStone",new Color(.38f,.4f,.34f),0,.15f,stoneLayer.diffuseTexture,stoneLayer.normalMapTexture);
            foreach(var r in statue.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>stone).ToArray();statue.AddComponent<MonumentPose>();
            var monumentCollider=statue.AddComponent<CapsuleCollider>();monumentCollider.center=Vector3.up;monumentCollider.height=2;statue.layer=8;
            var monumentBlock=new GameObject("Monument navigation exclusion");monumentBlock.transform.SetParent(root);monumentBlock.transform.position=new Vector3(0,2,36);var exclusion=monumentBlock.AddComponent<NavMeshModifierVolume>();exclusion.area=1;exclusion.size=new Vector3(2.8f,5,2.8f);
            // Each authored stop has a recognizable kit composition and clear central combat lane.
            for(int n=3;n<Oathfall.Core.ExpeditionContent.Stops.Length;n++)
            {
                var stop=Oathfall.Core.ExpeditionContent.Stops[n];var center=new Vector3(stop.X,0,stop.Z);
                Place("@ShrineStoneArch.fbx",center+Vector3.forward*12,4.2f,n%2*18,true);
                if(stop.Kind==Oathfall.Core.EncounterKind.Ward)
                {
                    for(int j=0;j<7;j++){float a=j*Mathf.PI*2/7;Place("@WoodlandBoulder"+(j%3)+".fbx",center+new Vector3(Mathf.Cos(a)*11,0,Mathf.Sin(a)*11),1.2f,j*31,true);}
                    Place("Nature/DeadTree_1.fbx",center+Vector3.left*12,6,n*29,true);
                }
                else if(stop.Kind==Oathfall.Core.EncounterKind.Sanctuary)
                {
                    Place("Village/Prop_Wagon.fbx",center+Vector3.right*8,2.1f,60,true);
                    for(int j=0;j<4;j++)Place("Village/Prop_Crate.fbx",center+new Vector3(6+j,0,-5),.8f,j*17,true);
                }
                else
                {
                    for(int j=0;j<3;j++)Place("@ShrineBrokenWall.fbx",center+new Vector3(-10+j*10,0,11),2.4f,j*28,true);
                    Place("Village/Prop_Wagon.fbx",center+Vector3.left*11,2.2f,n*24,true);
                    for(int j=0;j<8;j++)Place("Village/Prop_Brick"+(1+j%4)+".fbx",center+new Vector3(-9+j*.8f,0,-7),.25f,j*37,false);
                }
            }
            Place("@WoodlandBoulder0.fbx",new Vector3(0,0,484),3,0,true);
            var beacon=new GameObject("Northern waystone beacon",typeof(Light));beacon.transform.position=new Vector3(0,3,483);var beaconLight=beacon.GetComponent<Light>();beaconLight.color=new Color(1,.68f,.3f);beaconLight.range=12;beaconLight.intensity=5;
            var context=root.gameObject.AddComponent<HollowwoodSceneContext>();var spawn=new GameObject("Expedition Start").transform;spawn.SetParent(root);spawn.position=new Vector3(0,.1f,-43);
            Set(context,"heroSpawn",spawn);Set(context,"dawnwarden",AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>("Assets/Oathfall/Art/Dawnwarden.asset"));Set(context,"raider",AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>("Assets/Oathfall/Art/HollowwoodRaider.asset"));
            foreach(var role in new[]{"hexblade","breaker","guardian"})Set(context,role,AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(Art+"/"+role+".asset"));
            var loot=PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ExpeditionAssets.Third+"/Village/Prop_Crate.fbx")) as GameObject;loot.name="Equipment Cache";Remap(loot);Fit(loot,.35f);Set(context,"lootPrefab",PrefabUtility.SaveAsPrefabAsset(loot,Art+"/EquipmentDrop.prefab"));UnityEngine.Object.DestroyImmediate(loot);
            var cache=Place("Village/Prop_Crate.fbx",new Vector3(-12,0,14),1.2f,20,true);cache.name="Lost expedition cache";Set(context,"optionalCache",cache.transform);
            var gate=new GameObject("Camp shortcut gate");gate.transform.SetParent(root);gate.transform.position=new Vector3(-12,0,21);var fence=Place("Village/Prop_WoodenFence_Single.fbx",gate.transform.position,1.7f,0,true);fence.transform.SetParent(gate.transform,true);fence.isStatic=false;var obstacle=gate.AddComponent<NavMeshObstacle>();obstacle.carving=true;obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=Vector3.up;obstacle.size=new Vector3(4,2,.5f);Set(context,"shortcutGate",gate);
            foreach(var boundary in new[]{new Vector3(-44,2,220),new Vector3(44,2,220),new Vector3(0,2,-55),new Vector3(0,2,495)}){var wall=new GameObject("Forest boundary");wall.transform.SetParent(root);wall.transform.position=boundary;wall.layer=8;wall.AddComponent<BoxCollider>().size=boundary.x!=0?new Vector3(2,12,570):new Vector3(110,12,2);}
            var reflections=new GameObject("Shrine reflection",typeof(ReflectionProbe));reflections.transform.position=new Vector3(0,3,24);var probe=reflections.GetComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.size=new Vector3(70,30,110);probe.resolution=128;probe.intensity=.65f;probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=new Color(.32f,.42f,.48f);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=spawn.position+new Vector3(0,14,-12);camera.transform.rotation=Quaternion.Euler(49,0,0);var cam=camera.GetComponent<Camera>();cam.fieldOfView=45;cam.farClipPlane=140;cam.allowHDR=true;var urp=cam.GetUniversalAdditionalCameraData();urp.renderPostProcessing=true;urp.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var sun=new GameObject("Late afternoon through the canopy",typeof(Light)).GetComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.55f;sun.color=new Color(1,.9f,.74f);sun.transform.rotation=Quaternion.Euler(38,-48,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.85f;RenderSettings.sun=sun;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.52f,.62f,.7f);RenderSettings.ambientEquatorColor=new Color(.34f,.43f,.39f);RenderSettings.ambientGroundColor=new Color(.18f,.23f,.18f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.018f;RenderSettings.fogColor=new Color(.22f,.31f,.32f);
            var ambient=new SphericalHarmonicsL2();ambient.AddAmbientLight(new Color(.21f,.27f,.29f));RenderSettings.ambientProbe=ambient;
            var fill=new GameObject("Cool canopy bounce",typeof(Light)).GetComponent<Light>();fill.type=LightType.Directional;fill.color=new Color(.7f,.82f,1);fill.intensity=.4f;fill.transform.rotation=Quaternion.Euler(55,155,0);fill.shadows=LightShadows.None;
            groundLayer.diffuseRemapMax=new Vector4(.72f,.8f,.72f,1);stoneLayer.diffuseRemapMax=new Vector4(.7f,.74f,.69f,1);stoneLayer.smoothness=.05f;EditorUtility.SetDirty(stoneLayer);mossLayer.diffuseRemapMax=new Vector4(.64f,.77f,.58f,1);EditorUtility.SetDirty(groundLayer);EditorUtility.SetDirty(mossLayer);
            var profile=WoodlandAtmosphereAuthoring.LightingProfile(Art+"/HollowwoodLighting.asset");var volume=new GameObject("Hollowwood Atmosphere",typeof(Volume)).GetComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
            WoodlandAtmosphereAuthoring.Create(root);
            // Navigation must not promise steps the CharacterController cannot climb.
            var navigationSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);
            var humanoid=navigationSettings.FindProperty("m_Settings").GetArrayElementAtIndex(0);
            humanoid.FindPropertyRelative("agentClimb").floatValue=.3f;
            humanoid.FindPropertyRelative("agentRadius").floatValue=.6f;
            navigationSettings.ApplyModifiedPropertiesWithoutUndo();
            UnityEngine.Physics.SyncTransforms();var nav=root.gameObject.AddComponent<NavMeshSurface>();nav.collectObjects=CollectObjects.Children;nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.layerMask=1<<8;nav.overrideVoxelSize=true;nav.voxelSize=.15f;nav.BuildNavMesh();
            // Native NavMesh payload cannot be replaced with CopySerialized reliably.
            var baked=nav.navMeshData;nav.RemoveData();AssetDatabase.DeleteAsset(Art+"/HollowwoodNavigation.asset");AssetDatabase.CreateAsset(baked,Art+"/HollowwoodNavigation.asset");nav.navMeshData=baked;nav.AddData();
            foreach(var center in Oathfall.Gameplay.OathfallGame.Centers)if(!NavMesh.SamplePosition(center,out _,2,NavMesh.AllAreas))throw new InvalidOperationException("Missing encounter navigation: "+center);
            var route=new[]{new Vector3(0,0,-43)}.Concat(Oathfall.Gameplay.OathfallGame.Centers).Concat(new[]{new Vector3(0,0,480)}).ToArray();
            for(int i=1;i<route.Length;i++){var path=new NavMeshPath();if(!NavMesh.CalculatePath(route[i-1],route[i],NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new InvalidOperationException("Incomplete expedition route to "+route[i]);}
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Hollowwood.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Hollowwood.unity",true)};AssetDatabase.SaveAssets();Debug.Log("Connected Hollowwood authored and navigation baked");
        }
        private static Texture2D Texture(string folder,string token)
        {
            var path=Directory.GetFiles(folder).Where(p=>!p.EndsWith(".meta")&&Path.GetFileName(p).Contains(token)).Select(p=>p.Replace('\\','/')).First(p=>AssetDatabase.LoadAssetAtPath<Texture2D>(p)!=null);
            // Terrain interprets albedo alpha as smoothness. These opaque scans have no authored wetness.
            if(token=="diff" && AssetImporter.GetAtPath(path) is TextureImporter importer && importer.alphaSource!=TextureImporterAlphaSource.None){importer.alphaSource=TextureImporterAlphaSource.None;importer.SaveAndReimport();}
            if(path.Contains("_nor_dx_") && AssetImporter.GetAtPath(path) is TextureImporter normalImporter && !normalImporter.flipGreenChannel){normalImporter.flipGreenChannel=true;normalImporter.SaveAndReimport();}
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        private static T Save<T>(T asset,string path) where T:UnityEngine.Object {var previous=AssetDatabase.LoadAssetAtPath<T>(path);if(previous==null){AssetDatabase.CreateAsset(asset,path);return asset;}EditorUtility.CopySerialized(asset,previous);UnityEngine.Object.DestroyImmediate(asset);return previous;}
        private static void Set(UnityEngine.Object target,string name,UnityEngine.Object value){var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private static GameObject Place(string source,Vector3 position,float height,float yaw,bool collision)
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(source.StartsWith("@")?Art+"/Models/"+source.Substring(1):ExpeditionAssets.Third+"/"+source);if(prefab==null)throw new Exception("Missing environment model "+source);
            var obj=(GameObject)PrefabUtility.InstantiatePrefab(prefab);obj.transform.SetParent(root);Fit(obj,height);
            if(source.StartsWith("@"))
            {
                var wrapper=new GameObject(obj.name);wrapper.transform.SetParent(root);obj.transform.SetParent(wrapper.transform,true);
                var measured=obj.GetComponentsInChildren<Renderer>()[0].bounds;foreach(var renderer in obj.GetComponentsInChildren<Renderer>())measured.Encapsulate(renderer.bounds);
                obj.transform.position-=new Vector3(measured.center.x,measured.min.y,measured.center.z);wrapper.transform.position=new Vector3(position.x,terrain.SampleHeight(position)+terrain.transform.position.y,position.z);wrapper.transform.rotation=Quaternion.Euler(0,yaw,0);Remap(obj);
                if(collision)foreach(var filter in obj.GetComponentsInChildren<MeshFilter>()){filter.gameObject.layer=8;filter.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;}
                if(source.Contains("StoneArch")||source.Contains("BrokenWall"))wrapper.AddComponent<CameraOccluder>();
                if(source.Contains("Boulder")){wrapper.layer=8;var modifier=wrapper.AddComponent<NavMeshModifier>();modifier.overrideArea=true;modifier.area=1;}
                return wrapper;
            }
            var bounds=obj.GetComponentsInChildren<Renderer>()[0].bounds;foreach(var r in obj.GetComponentsInChildren<Renderer>())bounds.Encapsulate(r.bounds);
            obj.transform.position+=Vector3.up*(-bounds.min.y);
            obj.transform.position+=new Vector3(position.x,terrain.SampleHeight(position)+terrain.transform.position.y,position.z);obj.transform.rotation=Quaternion.Euler(0,yaw,0)*obj.transform.rotation;obj.isStatic=true;Remap(obj);
            if(collision)foreach(var filter in obj.GetComponentsInChildren<MeshFilter>()){filter.gameObject.layer=8;filter.gameObject.AddComponent<MeshCollider>().sharedMesh=filter.sharedMesh;}
            return obj;
        }
        private static void Fit(GameObject obj,float height){var renderers=obj.GetComponentsInChildren<Renderer>();if(renderers.Length==0)return;var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);obj.transform.localScale*=height/Mathf.Max(.01f,bounds.size.y);}
        private static void Remap(GameObject obj)
        {
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>())renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>MaterialFor(m)).ToArray();
        }
        private static Material MaterialFor(Material original)
        {
            string name=original!=null?original.name:"Bark";if(materials.TryGetValue(name,out var found))return found;
            if(name.StartsWith("Canopy_")||name=="Flower_Ivory")
            {
                var foliage=ExpeditionAssets.Lit(name,name=="Canopy_Shadow"?new Color(.23f,.31f,.13f):name=="Canopy_Mid"?new Color(.34f,.43f,.19f):name=="Canopy_Light"?new Color(.43f,.49f,.24f):new Color(.68f,.62f,.42f),0,.1f);foliage.SetFloat("_Cull",0);
                // A restrained transmission approximation keeps thin shaded leaves readable.
                foliage.SetColor("_EmissionColor",foliage.color*.045f);materials[name]=foliage;return foliage;
            }
            if(name=="Oath_Fern") {var fern=ExpeditionAssets.Lit("WoodlandFern",new Color(.22f,.34f,.13f),0,.28f);fern.SetFloat("_Cull",0);materials[name]=fern;return fern;}
            if(name=="Oath_ForestRock") {var rock=ExpeditionAssets.Lit("WoodlandBoulder",new Color(.55f,.59f,.53f),0,.12f,Texture("Assets/Oathfall/ThirdParty/PolyHaven/RockBoulderCracked_2K","diff"),Texture("Assets/Oathfall/ThirdParty/PolyHaven/RockBoulderCracked_2K","nor_gl"));materials[name]=rock;return rock;}
            if(name=="Oath_RootBark") {var bark=ExpeditionAssets.Lit("WoodlandRoots",new Color(.47f,.43f,.35f),0,.15f,AssetDatabase.LoadAssetAtPath<Texture2D>(ExpeditionAssets.Third+"/Nature/NormalTree_Bark.png"),AssetDatabase.LoadAssetAtPath<Texture2D>(ExpeditionAssets.Third+"/Nature/NormalTree_Bark_Normal.png"));materials[name]=bark;return bark;}
            if(name.Contains("Oath_WeatheredStone")){var rock=ExpeditionAssets.Lit("Weathered shrine masonry",new Color(.55f,.59f,.53f),0,.18f,Texture("Assets/Oathfall/ThirdParty/PolyHaven/Cobblestone01_2K","diff"),Texture("Assets/Oathfall/ThirdParty/PolyHaven/Cobblestone01_2K","nor"));materials[name]=rock;return rock;}
            string token=name.Replace("MI_","").Replace("M_","").Replace("_Wear","").Replace("Material","");
            string[] paths=AssetDatabase.FindAssets("t:Texture2D",new[]{ExpeditionAssets.Third+"/Village/Textures",ExpeditionAssets.Third+"/Nature"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            string texPath=paths.FirstOrDefault(p=>Path.GetFileNameWithoutExtension(p)==token);
            if(texPath==null)texPath=paths.FirstOrDefault(p=>Path.GetFileNameWithoutExtension(p)=="T_"+token+"_BaseColor");
            if(texPath==null)texPath=paths.FirstOrDefault(p=>Path.GetFileNameWithoutExtension(p).Equals("T_"+token,StringComparison.OrdinalIgnoreCase));
            var tex=texPath==null?null:AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            var tint=name.Contains("Leaves")?new Color(.28f,.34f,.24f):name.Contains("Grass")?new Color(.42f,.5f,.32f):tex!=null?new Color(.83f,.85f,.79f):new Color(.34f,.32f,.25f);
            var material=ExpeditionAssets.Lit("Environment_"+name,tint,name.Contains("Metal")?.5f:0,.18f,tex);
            if(name.Contains("Leaves")||name.Contains("Grass")||name.Contains("Vine")){material.SetFloat("_AlphaClip",1);material.SetFloat("_Cutoff",.4f);material.SetFloat("_Cull",0);material.EnableKeyword("_ALPHATEST_ON");material.renderQueue=2450;}
            materials[name]=material;return material;
        }
    }
}


