using Oathfall.Core;
using NUnit.Framework;

namespace Oathfall.Tests.EditMode
{
    public sealed class ItemGeneratorTests
    {
        [Test]
        public void Generate_WithSameSeed_ReturnsSameItemData()
        {
            var generator = new ItemGenerator();
            var first = generator.Generate(HeroId.Dawnwarden, 12, 8128u);
            var second = generator.Generate(HeroId.Dawnwarden, 12, 8128u);

            Assert.That(first.InstanceId, Is.Not.EqualTo(second.InstanceId));
            Assert.That(first.BaseId, Is.EqualTo(second.BaseId));
            Assert.That(first.Rarity, Is.EqualTo(second.Rarity));
            Assert.That(first.Affixes.Count, Is.EqualTo(second.Affixes.Count));
            for (var index = 0; index < first.Affixes.Count; index++)
            {
                Assert.That(first.Affixes[index].AffixId, Is.EqualTo(second.Affixes[index].AffixId));
                Assert.That(first.Affixes[index].Value, Is.EqualTo(second.Affixes[index].Value));
            }
        }

        [Test]
        public void Generate_ProducesUniqueAffixesWithinAnItem()
        {
            var generator = new ItemGenerator();
            for (uint seed = 1; seed < 500; seed++)
            {
                var item = generator.Generate(HeroId.Runebinder, 20, seed);
                for (var first = 0; first < item.Affixes.Count; first++)
                {
                    for (var second = first + 1; second < item.Affixes.Count; second++)
                    {
                        Assert.That(item.Affixes[first].AffixId, Is.Not.EqualTo(item.Affixes[second].AffixId));
                    }
                }
            }
        }

        [Test]
        public void DamageResolver_ArmorReducesButDoesNotNegateDamage()
        {
            var result = DamageResolver.Resolve(new DamageRequest { RawAmount = 100f }, 100f, 100f);
            Assert.That(result.FinalAmount, Is.EqualTo(50f).Within(0.001f));
            Assert.That(result.WasLethal, Is.False);
        }
    }
}
