using System;
using System.Collections.Generic;

namespace Oathfall.Core
{
    public enum HeroId
    {
        Dawnwarden,
        Thornstrider,
        Runebinder
    }

    public enum ItemSlot
    {
        Weapon,
        Offhand,
        Head,
        Body,
        Boots,
        Charm,
    }

    public enum ItemRarity
    {
        Common,
        Enchanted,
        Rare,
        Relic,
    }

    public enum DamageType
    {
        Physical,
        Fire,
        Frost,
        Arcane,
    }

    [Serializable]
    public struct DamageRequest
    {
        public string SourceId;
        public string AbilityId;
        public DamageType DamageType;
        public float RawAmount;
        public float ArmorPenetration;
    }

    [Serializable]
    public struct DamageResult
    {
        public float FinalAmount;
        public bool WasLethal;
    }

    public static class DamageResolver
    {
        public static DamageResult Resolve(DamageRequest request, float armor, float currentHealth)
        {
            var effectiveArmor = Math.Max(0f, armor - request.ArmorPenetration);
            var mitigation = 100f / (100f + effectiveArmor);
            var amount = Math.Max(0f, request.RawAmount) * mitigation;
            return new DamageResult
            {
                FinalAmount = amount,
                WasLethal = amount >= currentHealth,
            };
        }
    }

    public sealed class DeterministicRandom
    {
        private uint state;

        public DeterministicRandom(uint seed)
        {
            state = seed == 0 ? 0x6D2B79F5u : seed;
        }

        public uint NextUInt()
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        public int NextInt(int minimumInclusive, int maximumExclusive)
        {
            if (maximumExclusive <= minimumInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumExclusive));
            }

            return minimumInclusive + (int)(NextUInt() % (uint)(maximumExclusive - minimumInclusive));
        }

        public float NextFloat(float minimumInclusive, float maximumInclusive)
        {
            var normalized = NextUInt() / (float)uint.MaxValue;
            return minimumInclusive + ((maximumInclusive - minimumInclusive) * normalized);
        }
    }

    [Serializable]
    public struct AffixRoll
    {
        public string AffixId;
        public int Tier;
        public float Value;
    }

    [Serializable]
    public sealed class ItemInstance
    {
        public string InstanceId;
        public uint Seed;
        public HeroId IntendedHero;
        public ItemSlot Slot;
        public ItemRarity Rarity;
        public int ItemLevel;
        public string BaseId;
        public string RelicId;
        public List<AffixRoll> Affixes = new List<AffixRoll>();
    }
}
