using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Handles dodge mechanics with i-frames
    /// Based on GDD.md Combat Mechanics - Dodge System
    ///
    /// Dodge specifications:
    /// - Activation: Jump while strafing (A or D keys)
    /// - Distance: 2.5m quick displacement
    /// - Stamina cost: 15 points
    /// - Cooldown: 1.5 seconds
    /// - I-frames: 0.2 seconds invulnerability
    /// - Cannot dodge when overencumbered (>45kg) or prone
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(CharacterMovementSystem))]
    public partial class DodgeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (dodge, input, stamina, state, encumbrance) in
                     SystemAPI.Query<RefRW<DodgeData>, RefRO<PlayerInputData>, RefRW<StaminaData>,
                         RefRO<CharacterStateData>, RefRO<EncumbranceData>>())
            {
                // Update cooldown timer
                if (dodge.ValueRW.CooldownTimer > 0f)
                {
                    dodge.ValueRW.CooldownTimer -= deltaTime;
                }

                // Update i-frame timer
                if (dodge.ValueRW.IFrameTimer > 0f)
                {
                    dodge.ValueRW.IFrameTimer -= deltaTime;
                    dodge.ValueRW.HasIFrames = dodge.ValueRW.IFrameTimer > 0f;
                }

                // Check if currently dodging
                if (dodge.ValueRW.IsDodging)
                {
                    dodge.ValueRW.DodgeTimer += deltaTime;

                    // End dodge when duration is complete
                    if (dodge.ValueRW.DodgeTimer >= dodge.ValueRO.DodgeDuration)
                    {
                        dodge.ValueRW.IsDodging = false;
                        dodge.ValueRW.DodgeTimer = 0f;
                    }
                }
                // Check for dodge input (jump while strafing)
                else if (input.ValueRO.JumpPressedThisFrame &&
                         math.abs(input.ValueRO.MoveInput.x) > 0.1f)
                {
                    // Validate dodge conditions
                    bool canDodge = true;

                    // Check cooldown
                    if (dodge.ValueRO.CooldownTimer > 0f)
                        canDodge = false;

                    // Check stamina
                    if (stamina.ValueRO.Current < dodge.ValueRO.StaminaCost)
                        canDodge = false;

                    // Check if prone (cannot dodge while prone)
                    if (state.ValueRO.CurrentState == MovementState.Prone)
                        canDodge = false;

                    // Check encumbrance (cannot dodge when >45kg)
                    if (!encumbrance.ValueRO.CanDodge)
                        canDodge = false;

                    // Execute dodge
                    if (canDodge)
                    {
                        // Consume stamina
                        stamina.ValueRW.Current -= dodge.ValueRO.StaminaCost;

                        // Set dodge state
                        dodge.ValueRW.IsDodging = true;
                        dodge.ValueRW.DodgeTimer = 0f;
                        dodge.ValueRW.CooldownTimer = dodge.ValueRO.CooldownTime;

                        // Activate i-frames
                        dodge.ValueRW.HasIFrames = true;
                        dodge.ValueRW.IFrameTimer = dodge.ValueRO.IFrameDuration;

                        // Set dodge direction based on strafe input
                        dodge.ValueRW.DodgeDirectionX = math.sign(input.ValueRO.MoveInput.x);
                    }
                }
            }
        }
    }
}
