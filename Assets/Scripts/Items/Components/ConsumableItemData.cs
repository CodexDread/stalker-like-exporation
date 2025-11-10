using Unity.Entities;
using Unity.Collections;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Component for consumable items (food, medical, etc.)
    /// Based on GDD.md survival mechanics and medical system
    /// </summary>
    public struct ConsumableItemData : IComponentData
    {
        // Effects on consumption
        public float HealthRestore;           // HP restored (0 if none)
        public float StaminaRestore;          // Stamina restored
        public float HungerRestore;           // Hunger restored (0-100)
        public float ThirstRestore;           // Thirst restored (0-100)
        public float RadiationChange;         // Radiation added/removed (negative = removal)

        // Duration effects
        public float EffectDuration;          // How long effects last (0 = instant)
        public bool HasOverTime;              // Does this apply over time?

        // Side effects
        public float PoisonChance;            // Chance to cause poison (0.0-1.0)
        public float AddictionChance;         // Chance for addiction

        // Usage
        public float UseTime;                 // Time to consume (seconds)
        public bool CanUseInCombat;           // Can be used while in danger?
    }
}
