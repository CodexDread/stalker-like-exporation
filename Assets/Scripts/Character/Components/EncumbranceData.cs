using Unity.Entities;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Weight and encumbrance system
    /// Based on GDD.md Inventory System and Dodge System limitations
    ///
    /// Base carry weight: 30kg
    /// Maximum carry weight: 60kg
    /// Cannot dodge when overencumbered (>45kg)
    /// Skills can increase carry capacity (Strength and Endurance skills)
    /// </summary>
    public struct EncumbranceData : IComponentData
    {
        // Weight values (in kg)
        public float CurrentWeight;
        public float BaseMaxWeight;        // 30kg base
        public float AbsoluteMaxWeight;    // 60kg absolute maximum
        public float DodgeWeightLimit;     // 45kg - cannot dodge above this

        // Skill-based bonuses (from Endurance and Strength skills)
        public float SkillWeightBonus;     // Up to +15kg from each skill

        // Derived values
        public float EffectiveMaxWeight;   // BaseMaxWeight + SkillWeightBonus
        public bool IsOverencumbered;      // CurrentWeight > EffectiveMaxWeight
        public bool CanDodge;              // CurrentWeight <= DodgeWeightLimit

        // Movement penalties
        public float MovementSpeedMultiplier; // Reduced when heavily encumbered
    }
}
