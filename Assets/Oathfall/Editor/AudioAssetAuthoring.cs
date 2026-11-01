using System.IO;
using UnityEditor;
using UnityEngine;
namespace Oathfall.Editor
{
    public static class AudioAssetAuthoring
    {
        public static void Build()
        {
            string folder=ExpeditionAssets.Root+"/Resources/OathfallAudio";
            foreach(string file in Directory.GetFiles(folder))
            {
                if(file.EndsWith(".meta"))continue;string path=file.Replace('\\','/');var importer=AssetImporter.GetAtPath(path) as AudioImporter;if(importer==null)continue;
                var settings=importer.defaultSampleSettings;bool longClip=path.Contains("Forest")||path.Contains("OathTheme");settings.loadType=longClip?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=longClip?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;settings.quality=.8f;importer.defaultSampleSettings=settings;importer.forceToMono=!longClip;importer.SaveAndReimport();
            }
            foreach(string name in new[]{"Dawnwarden","HollowwoodRaider","hexblade","breaker","guardian"})
            {
                var def=AssetDatabase.LoadAssetAtPath<Oathfall.Presentation.ActorPresentationDefinition>(ExpeditionAssets.Root+"/"+name+".asset");var so=new SerializedObject(def);
                foreach(var pair in new[]{("swingSound","Swing.ogg"),("impactSound","Impact.ogg"),("stepSound","Step.ogg"),("blockSound","Block.ogg")})so.FindProperty(pair.Item1).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(folder+"/"+pair.Item2);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();
        }
    }
}
