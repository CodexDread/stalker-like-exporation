using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Manages stamina regeneration and consumption
    /// Based on GDD.md Character System - Stamina mechanics
    /// Affects sprint duration, dodge ability, and carrying capacity
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CharacterMovementSystem))]
    public partial class StaminaSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (stamina, state, encumbrance) in
                     SystemAPI.Query<RefRW<StaminaData>, RefRO<CharacterStateData>, RefRO<EncumbranceData>>())
            {
                // Calculate effective maximum stamina (base * multiplier from skills)
                float effectiveMax = stamina.ValueRO.Maximum * stamina.ValueRO.StaminaMultiplier;

                // Drain stamina when sprinting
                if (state.ValueRO.CurrentState == MovementState.Sprinting)
                {
                    stamina.ValueRW.Current -= stamina.ValueRO.SprintDrainRate * deltaTime;

                    // Prevent sprinting when stamina is depleted
                    if (stamina.ValueRW.Current <= 0f)
                    {
                        stamina.ValueRW.Current = 0f;
                        stamina.ValueRW.IsExhausted = true;
                    }
                }
                // Regenerate stamina when not sprinting
                else
                {
                    // Slower regen when moving, faster when standing still
                    float regenMultiplier = state.ValueRO.CurrentState == MovementState.Idle ? 1.5f : 1.0f;

                    // Additional penalty for being heavily encumbered
                    if (encumbrance.ValueRO.IsOverencumbered)
                    {
                        regenMultiplier *= 0.5f;
                    }

                    stamina.ValueRW.Current += stamina.ValueRO.RegenRate * regenMultiplier * deltaTime;

                    // Clamp to maximum
                    stamina.ValueRW.Current = math.min(stamina.ValueRW.Current, effectiveMax);

                    // Recover from exhaustion when stamina reaches threshold
                    if (stamina.ValueRW.IsExhausted &&
                        stamina.ValueRW.Current >= stamina.ValueRO.ExhaustedRecoveryThreshold)
                    {
                        stamina.ValueRW.IsExhausted = false;
                    }
                }
            }
        }
    }
}
