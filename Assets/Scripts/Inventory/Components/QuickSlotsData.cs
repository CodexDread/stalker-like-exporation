using Unity.Entities;

namespace ZoneSurvival.Inventory
{
    /// <summary>
    /// Quick slot data for fast access items
    /// Based on GDD.md quick slot system (lines 1112-1114)
    ///
    /// 1-0 keys: Weapons and equipment (10 slots)
    /// F1-F4 keys: Consumables (meds, food, grenades, etc.) (4 slots)
    /// </summary>
    public struct QuickSlot
    {
        public Entity ItemEntity;             // Entity of item in quick slot
        public int ItemID;                    // Item type ID
        public bool IsOccupied;               // Is slot filled?
        public bool IsEquipped;               // Is item currently equipped/active?
    }

    /// <summary>
    /// Quick slots component
    /// Fast access for weapons and consumables
    /// </summary>
    public struct QuickSlotsData : IComponentData
    {
        // Currently selected slots
        public int CurrentWeaponSlot;         // Which weapon slot is active (1-10, 0 = none)
        public int LastWeaponSlot;            // Previously selected weapon slot

        // State
        public bool IsWeaponDrawn;            // Is weapon currently out?
        public float WeaponSwitchTime;        // Time to switch weapons (1.0s default)
        public float SwitchProgress;          // Current switch progress
    }

    /// <summary>
    /// Buffer for weapon/equipment quick slots (1-0 keys)
    /// </summary>
    public struct WeaponQuickSlotBuffer : IBufferElementData
    {
        public QuickSlot Slot;
    }

    /// <summary>
    /// Buffer for consumable quick slots (F1-F4 keys)
    /// </summary>
    public struct ConsumableQuickSlotBuffer : IBufferElementData
    {
        public QuickSlot Slot;
    }
}
