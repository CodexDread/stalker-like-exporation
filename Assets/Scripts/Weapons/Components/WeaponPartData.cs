using Unity.Entities;
using Unity.Collections;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Individual weapon part with its own condition and performance modifiers
    /// Part of the two-stage durability system:
    /// - Overall weapon condition (in ItemData)
    /// - Individual part condition (this component)
    /// Both affect weapon performance and jamming chance
    /// </summary>
    public struct WeaponPartData : IComponentData
    {
        // Part Identity
        public int PartID;                          // Unique part identifier
        public FixedString64Bytes PartName;         // Display name
        public WeaponPartType PartType;             // What type of part this is
        public PartMountType MountType;             // How it attaches

        // Part Condition (0.0 = broken, 1.0 = pristine)
        public float Condition;                     // Current condition
        public float DegradationRate;               // Condition lost per use
        public float MaxCondition;                  // Maximum condition (can be < 1.0 if repaired poorly)

        // Part Weight
        public float Weight;                        // Kg

        // Performance Modifiers (additive with other parts)
        public float AccuracyModifier;              // +/- accuracy
        public float RecoilModifier;                // +/- recoil (negative = less recoil)
        public float RangeModifier;                 // +/- effective range
        public float ErgoModifier;                  // +/- ergonomics (ADS speed)
        public float DamageModifier;                // +/- damage

        // Jamming Contribution
        public float JamChanceWhenDegraded;         // Jam chance added when condition < 50%

        // Visual/Audio
        public int PrefabID;                        // For attaching 3D model
        public bool IsVisible;                      // Is this part visible on weapon?
    }

    /// <summary>
    /// Buffer of weapon parts attached to a weapon
    /// Allows variable number of parts per weapon
    /// </summary>
    public struct WeaponPartElement : IBufferElementData
    {
        public Entity PartEntity;                   // Reference to part entity
        public WeaponPartType SlotType;             // Which slot this part occupies
        public bool IsRequired;                     // Must be present for weapon to function
    }

    /// <summary>
    /// Defines which parts a weapon can accept
    /// Used for compatibility checking during assembly/disassembly
    /// </summary>
    public struct WeaponPartSlotDefinition : IBufferElementData
    {
        public WeaponPartType SlotType;             // Type of part this slot accepts
        public PartMountType RequiredMount;         // Required mount type
        public bool IsRequired;                     // Weapon won't function without this part
        public int MaxCount;                        // How many of this part type (e.g., multiple rails)
    }
}
