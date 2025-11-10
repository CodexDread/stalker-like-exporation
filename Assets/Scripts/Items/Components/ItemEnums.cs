using Unity.Entities;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Item categories for organization and game logic
    /// Based on GDD.md item systems and inventory organization
    /// </summary>
    public enum ItemCategory : byte
    {
        // Consumables
        Food = 0,
        Water = 1,
        Medical = 2,

        // Equipment
        Weapon = 10,
        Ammunition = 11,
        Armor = 12,
        Clothing = 13,

        // Tools & Devices
        Tool = 20,
        Detector = 21,
        Container = 22,

        // Survival
        Chemical = 30,
        CraftingMaterial = 31,

        // Special
        Artifact = 40,
        QuestItem = 41,

        // Misc
        Junk = 50,
        Currency = 51
    }

    /// <summary>
    /// Item rarity affects color coding and value
    /// </summary>
    public enum ItemRarity : byte
    {
        Common = 0,      // White/Gray
        Uncommon = 1,    // Green
        Rare = 2,        // Blue
        Epic = 3,        // Purple
        Legendary = 4,   // Orange
        Artifact = 5     // Red/Special (for zone artifacts)
    }

    /// <summary>
    /// Item usage type determines how the item is consumed
    /// </summary>
    public enum ItemUsageType : byte
    {
        None = 0,           // Cannot be used directly
        Instant = 1,        // Used immediately (food, medical)
        Equippable = 2,     // Can be equipped (weapons, armor)
        Deployable = 3,     // Placed in world (traps, containers)
        Readable = 4,       // Opens text/info (notes, documents)
        Craftable = 5       // Used in crafting recipes
    }
}
