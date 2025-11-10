using Unity.Entities;
using Unity.Collections;

namespace ZoneSurvival.Inventory
{
    /// <summary>
    /// Inventory grid slot data
    /// Based on GDD.md Tetris-style inventory system (line 1111)
    /// </summary>
    public struct InventorySlot
    {
        public Entity ItemEntity;             // Entity of item in this slot (Entity.Null if empty)
        public int ItemID;                    // Item type ID
        public int StackSize;                 // Number of items in stack
        public bool IsOccupied;               // Is this slot filled?
        public int OriginX;                   // X position of item origin (for multi-cell items)
        public int OriginY;                   // Y position of item origin
    }

    /// <summary>
    /// Main inventory data component
    /// Grid-based storage system with weight limits
    /// Based on GDD.md inventory specifications
    /// </summary>
    public struct InventoryData : IComponentData
    {
        // Grid dimensions
        public int GridWidth;                 // Number of columns (default: 10)
        public int GridHeight;                // Number of rows (default: 6)

        // Weight tracking (links to EncumbranceData)
        public float CurrentWeight;           // Total weight of all items
        public float MaxWeight;               // Maximum carry weight (from character)

        // Slot count
        public int TotalSlots;                // GridWidth * GridHeight
        public int OccupiedSlots;             // Number of slots with items
        public int FreeSlots;                 // Remaining empty slots

        // Money
        public int Currency;                  // Rubles (RU)
    }

    /// <summary>
    /// Buffer element for inventory grid
    /// Dynamic buffer allows flexible grid sizes
    /// </summary>
    public struct InventorySlotBuffer : IBufferElementData
    {
        public InventorySlot Slot;
    }
}
