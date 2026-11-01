#if UNITY_EDITOR
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;

namespace Oathfall.Editor
{
    [InitializeOnLoad]
    public static class TmpResourceBootstrap
    {
        static TmpResourceBootstrap()
        {
            EditorApplication.delayCall += ImportWhenNeeded;
        }

        private static void ImportWhenNeeded()
        {
            const string settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(settingsPath) == null)
            {
                TMP_PackageResourceImporter.ImportResources(true, false, false);
            }
        }
    }
}
#endif
