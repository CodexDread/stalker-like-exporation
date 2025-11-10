using Unity.Entities;
using Unity.Collections;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Core item data component
    /// Contains all essential item properties
    /// Based on GDD.md inventory system (lines 1111-1119)
    /// </summary>
    public struct ItemData : IComponentData
    {
        // Identification
        public int ItemID;                    // Unique item type ID
        public FixedString64Bytes ItemName;   // Display name (e.g., "AK-74M", "Bandage")

        // Category and type
        public ItemCategory Category;
        public ItemRarity Rarity;
        public ItemUsageType UsageType;

        // Physical properties
        public float Weight;                  // Weight in kg (affects encumbrance)
        public int GridWidth;                 // Width in inventory grid (Tetris-style)
        public int GridHeight;                // Height in inventory grid

        // Stack properties
        public bool IsStackable;              // Can multiple items stack in one slot?
        public int MaxStackSize;              // Maximum stack count (0 = infinite, 1 = not stackable)
        public int CurrentStackSize;          // Current stack count

        // Value
        public int BaseValue;                 // Base price in Rubles (RU)

        // Condition (for degradable items)
        public bool HasCondition;             // Does this item degrade?
        public float Condition;               // 0.0 to 1.0 (100%)
        public float MaxCondition;            // Maximum condition value

        // Usage flags
        public bool IsQuestItem;              // Cannot be dropped/sold
        public bool IsExamined;               // Has player inspected this item?
    }
}
