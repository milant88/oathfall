using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Oathfall.Tests.EditMode
{
    public class LightingPersistenceTests
    {
        private const string Path="Assets/__OathfallLightingRegression.asset";
        [TearDown] public void Cleanup()=>AssetDatabase.DeleteAsset(Path);
        [Test] public void LightingSurvivesReimportAndRepeatedAuthoringWithoutDuplicateOverrides()
        {
            Oathfall.Editor.WoodlandAtmosphereAuthoring.LightingProfile(Path);
            AssetDatabase.ImportAsset(Path,ImportAssetOptions.ForceUpdate);
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Path);
            Assert.That(profile.components.Count,Is.EqualTo(4));
            Assert.That(profile.components.All(EditorUtility.IsPersistent),Is.True,"Transient overrides disappear from standalone builds.");
            Assert.That(profile.TryGet<Tonemapping>(out var tone),Is.True);Assert.That(tone.mode.overrideState,Is.True);Assert.That(tone.mode.value,Is.EqualTo(TonemappingMode.ACES));
            Oathfall.Editor.WoodlandAtmosphereAuthoring.LightingProfile(Path);
            Assert.That(AssetDatabase.LoadAllAssetsAtPath(Path).OfType<VolumeComponent>().Count(),Is.EqualTo(4));
        }
    }
}
