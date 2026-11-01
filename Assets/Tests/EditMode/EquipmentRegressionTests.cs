using NUnit.Framework;
using Oathfall.Core;

namespace Oathfall.Tests.EditMode
{
    public class InventoryTests
    {
        private static ItemInstance Item(string id) => new ItemInstance { InstanceId = id, Slot = ItemSlot.Weapon, ItemLevel = 1 };
        [Test] public void ComparisonPredictsSwapWithoutMutatingEquipment()
        {
            var bag=new InventoryState();var old=Item("old");bag.TryCollect(old);bag.Equip(0);
            var next=Item("next");next.ItemLevel=4;next.Affixes.Add(new AffixRoll{AffixId="damage",Value=7});
            var predicted=EquipmentStats.PreviewSwap(bag.Equipment,next);
            Assert.That(predicted.Damage,Is.EqualTo(45));Assert.That(bag.Equipment[ItemSlot.Weapon],Is.SameAs(old));
            bag.TryCollect(next);bag.Equip(0);Assert.That(EquipmentStats.Calculate(bag.Equipment).Damage,Is.EqualTo(predicted.Damage));
        }
        [Test] public void FullInventoryRetainsOwnershipAndSwapStillWorks()
        {
            var bag = new InventoryState();
            bag.TryCollect(Item("equipped")); bag.Equip(0);
            for (int i=0;i<24;i++) Assert.That(bag.TryCollect(Item("item"+i)), Is.EqualTo(PickupResult.Collected));
            Assert.That(bag.TryCollect(Item("overflow")), Is.EqualTo(PickupResult.Full));
            Assert.That(bag.Unequip(ItemSlot.Weapon), Is.False);
            Assert.That(bag.Equip(0), Is.True);
            Assert.That(bag.Items.Count, Is.EqualTo(24));
            Assert.That(bag.Items[0].InstanceId, Is.EqualTo("equipped"));
        }
        [Test] public void EquippedOrDroppedIdentityCannotBeCollectedAgain()
        {
            var bag = new InventoryState(); var item=Item("once");
            bag.TryCollect(item); bag.Equip(0);
            Assert.That(bag.TryCollect(item), Is.EqualTo(PickupResult.Duplicate));
        }
        [Test] public void EquipUnequipDoesNotAccumulateStats()
        {
            var bag = new InventoryState(); var item=Item("sword");
            item.Affixes.Add(new AffixRoll { AffixId="damage", Value=10 });
            bag.TryCollect(item);
            for(int i=0;i<20;i++)
            {
                bag.Equip(0);
                Assert.That(EquipmentStats.Calculate(bag.Equipment).Damage, Is.EqualTo(42));
                bag.Unequip(ItemSlot.Weapon);
                Assert.That(EquipmentStats.Calculate(bag.Equipment).Damage, Is.EqualTo(26));
            }
        }
        [Test] public void PickupUsesGroundAnchorAndRejectsOtherFloor()
        {
            Assert.That(PickupRules.InReach(0, 1.45f), Is.False);
            Assert.That(PickupRules.InReach(1, .1f), Is.True);
            Assert.That(PickupRules.InReach(2.26f, 0), Is.False);
        }
        [Test] public void LargeFrameStillCrossesActiveWindow()
        {
            var timing = new AttackTiming(.18f,.14f,.24f);
            Assert.That(timing.CrossesActive(.1f,.5f), Is.True);
            Assert.That(timing.CrossesActive(.4f,.5f), Is.False);
        }
        [Test] public void GeneratorOnlyOffersImplementedEffects()
        {
            var generator = new ItemGenerator();
            for(uint seed=1;seed<1000;seed++)
            {
                var item=generator.Generate(HeroId.Dawnwarden,2,seed);
                Assert.That(item.Rarity,Is.Not.EqualTo(ItemRarity.Relic));
                foreach(var affix in item.Affixes)
                    CollectionAssert.Contains(new[]{"max-health","armor","damage","move-speed"}, affix.AffixId);
            }
        }
    }
}

