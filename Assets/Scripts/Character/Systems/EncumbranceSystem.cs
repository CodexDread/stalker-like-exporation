using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Manages weight and encumbrance calculations
    /// Based on GDD.md Inventory System and dodge limitations
    ///
    /// Weight limits:
    /// - Base: 30kg
    /// - Max: 60kg
    /// - Dodge limit: 45kg
    /// - Skills can add +15kg each (Endurance and Strength)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CharacterMovementSystem))]
    public partial class EncumbranceSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var encumbrance in SystemAPI.Query<RefRW<EncumbranceData>>())
            {
                // Calculate effective maximum weight with skill bonuses
                encumbrance.ValueRW.EffectiveMaxWeight = math.min(
                    encumbrance.ValueRO.BaseMaxWeight + encumbrance.ValueRO.SkillWeightBonus,
                    encumbrance.ValueRO.AbsoluteMaxWeight
                );

                // Check if overencumbered
                encumbrance.ValueRW.IsOverencumbered =
                    encumbrance.ValueRO.CurrentWeight > encumbrance.ValueRO.EffectiveMaxWeight;

                // Check if can dodge (weight limit is 45kg as per GDD)
                encumbrance.ValueRW.CanDodge =
                    encumbrance.ValueRO.CurrentWeight <= encumbrance.ValueRO.DodgeWeightLimit;

                // Calculate movement speed multiplier based on encumbrance
                // 100% speed at 0-30kg, linearly decreasing to 50% at max weight
                float weightRatio = encumbrance.ValueRO.CurrentWeight / encumbrance.ValueRO.EffectiveMaxWeight;

                if (weightRatio <= 1.0f)
                {
                    // Normal range: 100% to 75% speed
                    encumbrance.ValueRW.MovementSpeedMultiplier = math.lerp(1.0f, 0.75f, weightRatio);
                }
                else
                {
                    // Overencumbered: 75% to 50% speed
                    float overweightRatio = (encumbrance.ValueRO.CurrentWeight - encumbrance.ValueRO.EffectiveMaxWeight) /
                                           (encumbrance.ValueRO.AbsoluteMaxWeight - encumbrance.ValueRO.EffectiveMaxWeight);
                    encumbrance.ValueRW.MovementSpeedMultiplier = math.lerp(0.75f, 0.5f, math.saturate(overweightRatio));
                }
            }
        }
    }
}
