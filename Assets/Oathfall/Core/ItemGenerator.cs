using System;
using System.Collections.Generic;

namespace Oathfall.Core
{
    public sealed class ItemGenerator
    {
        private static readonly ItemSlot[] Slots =
        {
            ItemSlot.Weapon,
            ItemSlot.Offhand,
            ItemSlot.Head,
            ItemSlot.Body,
            ItemSlot.Boots,
            ItemSlot.Charm,
        };

        private static readonly string[] UniversalAffixes =
        {
            "max-health",
            "armor",
            "move-speed",
            "damage",
        };

        private static readonly Dictionary<HeroId, string[]> HeroAffixes = new Dictionary<HeroId, string[]>
        {
            { HeroId.Dawnwarden, new[] { "resolve-on-block", "cleave-radius", "shield-bash-wave" } },
            { HeroId.Thornstrider, new[] { "mark-duration", "arrow-split", "trap-spread" } },
            { HeroId.Runebinder, new[] { "mana-regain", "frost-follow", "ember-splash" } },
        };

        public ItemInstance Generate(HeroId hero, int itemLevel, uint seed)
        {
            if (itemLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(itemLevel));
            }

            var random = new DeterministicRandom(seed);
            var rarity = RollRarity(random);
            var item = new ItemInstance
            {
                Seed = seed,
                InstanceId = Guid.NewGuid().ToString("N"),
                IntendedHero = hero,
                Slot = Slots[random.NextInt(0, Slots.Length)],
                Rarity = rarity,
                ItemLevel = itemLevel,
            };

            item.BaseId = BuildBaseId(hero, item.Slot);

            var affixCount = GetAffixCount(rarity, random);
            var available = new List<string>(UniversalAffixes);
            for (var index = 0; index < affixCount && available.Count > 0; index++)
            {
                var choice = random.NextInt(0, available.Count);
                var id = available[choice];
                available.RemoveAt(choice);
                item.Affixes.Add(new AffixRoll
                {
                    AffixId = id,
                    Tier = Math.Min(5, 1 + (itemLevel / 8) + random.NextInt(0, 2)),
                    Value = random.NextFloat(2f, 6f) + (itemLevel * 0.25f),
                });
            }

            return item;
        }

        private static ItemRarity RollRarity(DeterministicRandom random)
        {
            var roll = random.NextInt(0, 100);
            if (roll < 24) return ItemRarity.Rare;
            if (roll < 62) return ItemRarity.Enchanted;
            return ItemRarity.Common;
        }

        private static int GetAffixCount(ItemRarity rarity, DeterministicRandom random)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return 0;
                case ItemRarity.Enchanted: return 1 + random.NextInt(0, 2);
                case ItemRarity.Rare: return 3 + random.NextInt(0, 2);
                case ItemRarity.Relic: return 1 + random.NextInt(0, 2);
                default: throw new ArgumentOutOfRangeException(nameof(rarity));
            }
        }

        private static string BuildBaseId(HeroId hero, ItemSlot slot)
        {
            var heroId = hero.ToString().ToLowerInvariant();
            return $"{heroId}-{slot.ToString().ToLowerInvariant()}-base";
        }

        private static string GetRelicId(HeroId hero)
        {
            switch (hero)
            {
                case HeroId.Dawnwarden: return "relic-sunbreakers-vow";
                case HeroId.Thornstrider: return "relic-branching-arrow";
                case HeroId.Runebinder: return "relic-emberfrost-signet";
                default: throw new ArgumentOutOfRangeException(nameof(hero));
            }
        }
    }
}
