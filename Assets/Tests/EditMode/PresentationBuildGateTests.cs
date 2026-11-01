using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using Oathfall.Presentation;
namespace Oathfall.Tests.EditMode
{
    public class PresentationBuildGateTests
    {
        [Test] public void CompletePresentationPassesBuildGate()
        {
            new Oathfall.Editor.ExpeditionBuildValidator().OnPreprocessBuild(null);
        }
        [Test] public void MissingOrIncompleteGearModelCatalogIsRejected()
        {
            var catalog=GearCatalog.Load();var original=catalog.models;
            try
            {
                catalog.models=new GameObject[6];Assert.Throws<BuildFailedException>(()=>new Oathfall.Editor.ExpeditionBuildValidator().OnPreprocessBuild(null));
                catalog.models=new GameObject[0];Assert.Throws<BuildFailedException>(()=>new Oathfall.Editor.ExpeditionBuildValidator().OnPreprocessBuild(null));
            }
            finally{catalog.models=original;}
        }
    }
}
