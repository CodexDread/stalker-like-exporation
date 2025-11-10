using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Handles jump mechanics with buffering and coyote time
    /// Based on GDD.md movement specifications
    ///
    /// Features:
    /// - Jump buffering: Press jump slightly before landing
    /// - Coyote time: Grace period after leaving ground
    /// - Stamina cost: Optional stamina consumption
    /// - Cooldown: Prevents spam jumping
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GroundDetectionSystem))]
    [UpdateBefore(typeof(CharacterMovementSystem))]
    public partial struct JumpSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (jumpData, input, movement, stamina, groundData, stateData) in
                     SystemAPI.Query<RefRW<JumpData>, RefRO<PlayerInputData>, RefRW<CharacterMovementData>,
                         RefRW<StaminaData>, RefRO<GroundDetectionData>, RefRO<CharacterStateData>>())
            {
                // Update jump buffer timer
                if (input.ValueRO.JumpPressedThisFrame)
                {
                    jumpData.ValueRW.JumpBufferCounter = jumpData.ValueRO.JumpBufferTime;
                }
                else if (jumpData.ValueRW.JumpBufferCounter > 0f)
                {
                    jumpData.ValueRW.JumpBufferCounter -= deltaTime;
                }

                // Check if jump should be executed
                bool shouldJump = false;

                // Jump buffer active and can jump
                if (jumpData.ValueRO.JumpBufferCounter > 0f && jumpData.ValueRO.CanJump)
                {
                    shouldJump = true;
                }

                // Additional check: Cannot jump while prone
                if (stateData.ValueRO.CurrentState == MovementState.Prone)
                {
                    shouldJump = false;
                }

                // Check stamina requirement
                if (shouldJump && jumpData.ValueRO.JumpStaminaCost > 0f)
                {
                    if (stamina.ValueRO.Current < jumpData.ValueRO.JumpStaminaCost)
                    {
                        shouldJump = false; // Not enough stamina
                    }
                }

                // Execute jump
                if (shouldJump)
                {
                    // Apply jump force
                    movement.ValueRW.Velocity.y = jumpData.ValueRO.JumpForce;

                    // Consume stamina
                    if (jumpData.ValueRO.JumpStaminaCost > 0f)
                    {
                        stamina.ValueRW.Current -= jumpData.ValueRO.JumpStaminaCost;
                    }

                    // Update jump state
                    jumpData.ValueRW.IsJumping = true;
                    jumpData.ValueRW.JumpsRemaining -= 1;
                    jumpData.ValueRW.JumpCooldownCounter = jumpData.ValueRO.JumpCooldown;
                    jumpData.ValueRW.JumpBufferCounter = 0f; // Clear buffer
                    jumpData.ValueRW.CoyoteTimeCounter = 0f; // Clear coyote time
                }

                // Reset jumping state when landed
                if (groundData.ValueRO.JustLanded && jumpData.ValueRO.IsJumping)
                {
                    jumpData.ValueRW.IsJumping = false;
                }
            }
        }
    }
}
