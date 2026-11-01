using Oathfall.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;

namespace Oathfall.Editor
{
    /// <summary>Creates the small, source-controlled visual asset library used by the vertical slice.</summary>
    public static class OathfallVisualAssetSeeder
    {
        private const string Root = "Assets/Oathfall/Presentation/Generated";
        private const string Materials = Root + "/Materials";
        private const string Definitions = Root + "/Definitions";
        private const string Animation = Root + "/Animation";

        [MenuItem("Oathfall/Authoring/Ensure Hollowwood Visual Assets")]
        public static void Ensure()
        {
            EnsureFolder("Assets/Oathfall");
            EnsureFolder("Assets/Oathfall/Presentation");
            EnsureFolder(Root);
            EnsureFolder(Materials);
            EnsureFolder(Definitions);
            EnsureFolder(Animation);

            var floor = CreateLit("Hollowwood_ShrineFloor", new Color(0.19f, 0.25f, 0.23f), 0.02f, 0.48f);
            var oldStone = CreateLit("Hollowwood_OldStone", new Color(0.25f, 0.29f, 0.29f), 0.02f, 0.38f);
            var mossStone = CreateLit("Hollowwood_MossStone", new Color(0.18f, 0.28f, 0.20f), 0.01f, 0.34f);
            var bark = CreateLit("Hollowwood_Bark", new Color(0.17f, 0.11f, 0.07f), 0.01f, 0.28f);
            var leaf = CreateLit("Hollowwood_Leaf", new Color(0.10f, 0.22f, 0.13f), 0f, 0.22f);
            var warmGold = CreateLit("Dawnwarden_Gold", new Color(0.72f, 0.40f, 0.09f), 0.8f, 0.58f);
            var ember = CreateLit("Oath_Ember", new Color(0.55f, 0.09f, 0.025f), 0.1f, 0.4f, new Color(1.2f, 0.11f, 0.015f));
            var armour = CreateLit("Dawnwarden_Armour", new Color(0.24f, 0.29f, 0.33f), 0.87f, 0.7f);
            var cloth = CreateLit("Dawnwarden_Cloth", new Color(0.18f, 0.085f, 0.045f), 0.02f, 0.22f);
            var skin = CreateLit("Raider_Skin", new Color(0.23f, 0.105f, 0.07f), 0f, 0.32f);
            var raiderLeather = CreateLit("Raider_Leather", new Color(0.13f, 0.07f, 0.04f), 0.02f, 0.22f);
            var rust = CreateLit("Raider_Rust", new Color(0.27f, 0.09f, 0.035f), 0.6f, 0.42f);
            var corruption = CreateLit("Raider_Corruption", new Color(0.19f, 0.03f, 0.25f), 0.15f, 0.35f, new Color(0.12f, 0.01f, 0.3f));
            var telegraph = CreateTransparent("Raider_Telegraph", new Color(0.85f, 0.08f, 0.025f, 0.33f));

            var dawnController = EnsureController("Dawnwarden_Presentation");
            var raiderController = EnsureController("Raider_Presentation");
            var palette = EnsureAsset<PresentationPalette>(Definitions + "/HollowwoodPalette.asset");
            Set(palette, "shrineFloor", floor); Set(palette, "oldStone", oldStone); Set(palette, "mossStone", mossStone);
            Set(palette, "bark", bark); Set(palette, "leaf", leaf); Set(palette, "warmGold", warmGold); Set(palette, "ember", ember);

            var dawn = EnsureAsset<ActorPresentationDefinition>(Definitions + "/Dawnwarden.asset");
            Set(dawn, "stableId", "hero.dawnwarden.presentation"); Set(dawn, "kind", ActorPresentationKind.Dawnwarden);
            Set(dawn, "animatorController", dawnController); Set(dawn, "skinMaterial", skin); Set(dawn, "primaryMaterial", armour);
            Set(dawn, "secondaryMaterial", cloth); Set(dawn, "metalMaterial", armour); Set(dawn, "accentMaterial", warmGold); Set(dawn, "telegraphMaterial", telegraph);

            var raider = EnsureAsset<ActorPresentationDefinition>(Definitions + "/HollowwoodRaider.asset");
            Set(raider, "stableId", "enemy.hollowwood.raider.presentation"); Set(raider, "kind", ActorPresentationKind.HollowwoodRaider);
            Set(raider, "animatorController", raiderController); Set(raider, "skinMaterial", skin); Set(raider, "primaryMaterial", raiderLeather);
            Set(raider, "secondaryMaterial", bark); Set(raider, "metalMaterial", rust); Set(raider, "accentMaterial", corruption); Set(raider, "telegraphMaterial", telegraph);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Oathfall Hollowwood visual assets are ready.");
        }

        [MenuItem("Oathfall/Authoring/Build Hollowwood Shrine Scene")]
        public static void BuildHollowwoodShrine()
        {
            Ensure();
            var existing = GameObject.Find("Hollowwood Shrine");
            if (existing != null) Object.DestroyImmediate(existing);

            var palette = AssetDatabase.LoadAssetAtPath<PresentationPalette>(Definitions + "/HollowwoodPalette.asset");
            var dawn = AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(Definitions + "/Dawnwarden.asset");
            var raider = AssetDatabase.LoadAssetAtPath<ActorPresentationDefinition>(Definitions + "/HollowwoodRaider.asset");
            var root = new GameObject("Hollowwood Shrine") { isStatic = true };
            var floor = Part(root.transform, "Shrine Foundation", PrimitiveType.Cube, Vector3.zero, new Vector3(34f, .45f, 34f), palette.ShrineFloor);
            floor.transform.position = new Vector3(0f, -.35f, 0f);

            for (var x = -4; x <= 4; x++) for (var z = -4; z <= 4; z++)
            {
                var material = ((x + z) & 1) == 0 ? palette.OldStone : palette.MossStone;
                var tile = Part(root.transform, "Worn Shrine Tile", PrimitiveType.Cube, new Vector3(x * 3.65f, -.06f, z * 3.65f), new Vector3(3.48f, .12f, 3.48f), material);
                tile.transform.localRotation = Quaternion.Euler(0f, ((x * 31 + z * 17) % 6) * 1.2f, 0f);
            }

            var monument = new GameObject("Fallen Oath Monument"); monument.transform.SetParent(root.transform, false); monument.isStatic = true;
            Part(monument.transform, "Broken Pedestal", PrimitiveType.Cylinder, new Vector3(0f, .55f, 11.8f), new Vector3(1.8f, .55f, 1.8f), palette.OldStone);
            var statue = Part(monument.transform, "Fallen Statue", PrimitiveType.Capsule, new Vector3(.5f, 1.05f, 11.8f), new Vector3(.62f, 1.3f, .62f), palette.MossStone);
            statue.transform.localRotation = Quaternion.Euler(0f, 0f, 62f);
            Part(monument.transform, "Oath Ember", PrimitiveType.Sphere, new Vector3(0f, 1.35f, 11.8f), Vector3.one * .28f, palette.Ember);

            var arch = new GameObject("Broken Entrance Arch"); arch.transform.SetParent(root.transform, false); arch.isStatic = true;
            Part(arch.transform, "Arch Left", PrimitiveType.Cube, new Vector3(-4.3f, 2.4f, 15.2f), new Vector3(1.05f, 4.8f, 1.15f), palette.OldStone);
            Part(arch.transform, "Arch Right", PrimitiveType.Cube, new Vector3(4.3f, 2.4f, 15.2f), new Vector3(1.05f, 4.8f, 1.15f), palette.OldStone);
            var lintel = Part(arch.transform, "Arch Lintel", PrimitiveType.Cube, new Vector3(0f, 4.5f, 15.2f), new Vector3(7.7f, 1.05f, 1.15f), palette.MossStone);
            lintel.transform.localRotation = Quaternion.Euler(0f, 0f, -7f);

            for (var i = 0; i < 8; i++)
            {
                var angle = i / 8f * Mathf.PI * 2f;
                var pos = new Vector3(Mathf.Cos(angle) * 14.2f, 1.6f, Mathf.Sin(angle) * 14.2f);
                var column = Part(root.transform, "Fractured Boundary Column", PrimitiveType.Cylinder, pos, new Vector3(.75f, 1.6f, .75f), i % 2 == 0 ? palette.OldStone : palette.MossStone);
                column.transform.localRotation = Quaternion.Euler(i % 3 * 8f, 0f, i * 9f);
                var rubble = Part(root.transform, "Column Rubble", PrimitiveType.Cube, pos + new Vector3(.9f, .18f, -.45f), new Vector3(1.2f, .36f, .7f), palette.OldStone);
                rubble.transform.localRotation = Quaternion.Euler(12f, i * 35f, 18f);
            }

            for (var i = 0; i < 12; i++)
            {
                var angle = i / 12f * Mathf.PI * 2f + .18f;
                var treePos = new Vector3(Mathf.Cos(angle) * 20.5f, 2.8f, Mathf.Sin(angle) * 20.5f);
                var trunk = Part(root.transform, "Ancient Hollowwood Tree", PrimitiveType.Cylinder, treePos, new Vector3(.95f, 2.8f, .95f), palette.Bark);
                trunk.transform.localRotation = Quaternion.Euler((i % 3 - 1) * 5f, 0f, (i % 2 == 0 ? 1f : -1f) * 8f);
                Part(root.transform, "Dense Canopy", PrimitiveType.Sphere, treePos + new Vector3(0f, 4.2f, 0f), new Vector3(3.8f, 2.1f, 3.8f), palette.Leaf);
            }

            var wagon = new GameObject("Abandoned Expedition Wagon"); wagon.transform.SetParent(root.transform, false); wagon.isStatic = true;
            Part(wagon.transform, "Wagon Bed", PrimitiveType.Cube, new Vector3(-11f, .7f, -8.5f), new Vector3(2.6f, .45f, 1.35f), palette.Bark);
            for (var i = 0; i < 4; i++) Part(wagon.transform, "Wagon Wheel", PrimitiveType.Cylinder, new Vector3(-11f + (i < 2 ? -1f : 1f), .38f, -8.5f + (i % 2 == 0 ? -.8f : .8f)), new Vector3(.55f, .16f, .55f), palette.WarmGold).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var spawn = new GameObject("Hero Spawn").transform; spawn.SetParent(root.transform, false); spawn.position = Vector3.zero;
            var context = root.AddComponent<HollowwoodSceneContext>();
            Set(context, "palette", palette); Set(context, "dawnwarden", dawn); Set(context, "raider", raider); Set(context, "heroSpawn", spawn);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("Hollowwood Shrine scene composition is ready.");
        }

        private static Material CreateLit(string name, Color baseColor, float metallic, float smoothness, Color? emission = null)
        {
            var path = Materials + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new System.InvalidOperationException("URP Lit shader was not found by the Editor.");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", baseColor);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateTransparent(string name, Color color)
        {
            var path = Materials + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) throw new System.InvalidOperationException("URP Unlit shader was not found by the Editor.");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static AnimatorController EnsureController(string name)
        {
            var path = Animation + "/" + name + ".controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller != null) return controller;

            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Action", AnimatorControllerParameterType.Int);
            controller.AddParameter("Move", AnimatorControllerParameterType.Float);
            var stateMachine = controller.layers[0].stateMachine;
            foreach (var stateName in new[] { "Idle", "Move", "AttackWindup", "AttackActive", "AttackRecover", "Block", "Dodge", "Hit", "Stagger", "Death" })
            {
                var clip = new AnimationClip { name = name + "_" + stateName };
                AssetDatabase.AddObjectToAsset(clip, controller);
                var state = stateMachine.AddState(stateName);
                state.motion = clip;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static T EnsureAsset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Set(Object target, string propertyName, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void Set(Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void Set(Object target, string propertyName, ActorPresentationKind value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).enumValueIndex = (int)value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var slash = path.LastIndexOf('/');
                AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
            }
        }

        private static GameObject Part(Transform parent, string name, PrimitiveType primitive, Vector3 position, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.isStatic = true;
            part.GetComponent<Renderer>().sharedMaterial = material;
            return part;
        }
    }
}
