using Unity.Entities;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Stamina system for character
    /// Based on GDD.md Character System (Health & Stamina)
    /// Affects sprint duration, carrying capacity, and dodge ability
    /// </summary>
    public struct StaminaData : IComponentData
    {
        // Stamina values
        public float Current;
        public float Maximum;

        // Stamina rates (per second)
        public float RegenRate;
        public float SprintDrainRate;
        public float DodgeCost;

        // State flags
        public bool IsExhausted; // Cannot sprint when true
        public float ExhaustedRecoveryThreshold; // Stamina amount needed to recover from exhaustion

        // Multipliers based on skills (from GDD Endurance skill)
        public float StaminaMultiplier; // 1.0 base, up to 1.75 at max skill
    }
}
