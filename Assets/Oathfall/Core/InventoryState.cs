using System;
using System.Collections.Generic;

namespace Oathfall.Core
{
    public enum PickupResult { Collected, Full, Duplicate, Invalid }

    public sealed class EquipmentState
    {
        private readonly ItemInstance[] slots = new ItemInstance[6];
        public ItemInstance this[ItemSlot slot] => slots[(int)slot];
        public IEnumerable<ItemInstance> Items => slots;
        internal void Set(ItemSlot slot, ItemInstance item) => slots[(int)slot] = item;
    }

    public sealed class InventoryState
    {
        public const int Capacity = 24;
        private readonly List<ItemInstance> items = new List<ItemInstance>();
        private readonly HashSet<string> collected = new HashSet<string>();
        public IReadOnlyList<ItemInstance> Items => items;
        public EquipmentState Equipment { get; } = new EquipmentState();
        public PickupResult TryCollect(ItemInstance item)
        {
            if (item == null || string.IsNullOrEmpty(item.InstanceId) || !Enum.IsDefined(typeof(ItemSlot), item.Slot)) return PickupResult.Invalid;
            if (collected.Contains(item.InstanceId)) return PickupResult.Duplicate;
            if (items.Count >= Capacity) return PickupResult.Full;
            items.Add(item);
            collected.Add(item.InstanceId);
            return PickupResult.Collected;
        }
        public bool Discard(int index)
        {
            if(index<0||index>=items.Count)return false;
            items.RemoveAt(index);return true; // Keep collected identity: discarded items cannot be claimed twice.
        }
        public bool Equip(int index)
        {
            if (index < 0 || index >= items.Count) return false;
            var next = items[index];
            var previous = Equipment[next.Slot];
            if (previous == null) items.RemoveAt(index); else items[index] = previous;
            Equipment.Set(next.Slot, next);
            return true;
        }
        public bool Unequip(ItemSlot slot)
        {
            var item = Equipment[slot];
            if (item == null || items.Count >= Capacity) return false;
            items.Add(item);
            Equipment.Set(slot, null);
            return true;
        }
    }

    public struct HeroStats
    {
        public float Health, Armor, Damage, MoveSpeed;
        public static HeroStats Baseline => new HeroStats { Health = 150, Armor = 22, Damage = 26, MoveSpeed = 5.2f };
    }

    public static class EquipmentStats
    {
        public static HeroStats PreviewSwap(EquipmentState equipment, ItemInstance replacement)
        {
            var preview=new EquipmentState();
            for(int i=0;i<6;i++)preview.Set((ItemSlot)i, replacement!=null && replacement.Slot==(ItemSlot)i ? replacement : equipment[(ItemSlot)i]);
            return Calculate(preview);
        }

        public static HeroStats Calculate(EquipmentState equipment)
        {
            var stats = HeroStats.Baseline;
            float speedPercent = 0;
            foreach (var item in equipment.Items)
            {
                if (item == null) continue;
                if (item.Slot == ItemSlot.Weapon) stats.Damage += 4 + item.ItemLevel * 2;
                else if (item.Slot == ItemSlot.Charm) stats.Health += 8 + item.ItemLevel * 2;
                else stats.Armor += 2 + item.ItemLevel;
                foreach (var affix in item.Affixes)
                {
                    switch (affix.AffixId)
                    {
                        case "max-health": stats.Health += affix.Value; break;
                        case "armor": stats.Armor += affix.Value; break;
                        case "damage": stats.Damage += affix.Value; break;
                        case "move-speed": speedPercent += affix.Value; break;
                    }
                }
            }
            stats.MoveSpeed *= 1 + Math.Min(30, speedPercent) / 100;
            return stats;
        }
    }

    public static class PickupRules
    {
        public static bool InReach(float planarDistanceSquared, float heightDifference) =>
            planarDistanceSquared <= 2.25f && Math.Abs(heightDifference) <= .75f;
    }
}
