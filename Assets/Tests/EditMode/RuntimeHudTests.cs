using NUnit.Framework;
using Oathfall.Gameplay;
using UnityEngine;

namespace Oathfall.Tests.EditMode
{
    public sealed class RuntimeHudTests
    {
        [Test]
        public void HeroSelectionHidesRunHudUntilANewRunStarts()
        {
            var hud = RuntimeHud.Create(_ => { });

            hud.HideHeroSelection();
            Assert.That(hud.IsRunHudVisible, Is.True);

            hud.ShowHeroSelection("A prior run ended.");
            Assert.That(hud.IsRunHudVisible, Is.False);

            var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            Object.DestroyImmediate(hud.gameObject);
            if (eventSystem != null)
            {
                Object.DestroyImmediate(eventSystem.gameObject);
            }
        }
    }
}
