using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Main character movement system
    /// Handles WASD movement, state transitions, and physics
    /// Based on GDD.md Controls and Movement specifications
    ///
    /// Movement states: Idle, Walking, Sprinting, Crouching, Prone
    /// Controls: WASD + sprint/crouch/prone
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(StaminaSystem))]
    [UpdateAfter(typeof(EncumbranceSystem))]
    [UpdateAfter(typeof(DodgeSystem))]
    public partial class CharacterMovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (movement, state, input, stamina, encumbrance, dodge, camera, transform) in
                     SystemAPI.Query<RefRW<CharacterMovementData>, RefRW<CharacterStateData>, RefRO<PlayerInputData>,
                         RefRO<StaminaData>, RefRO<EncumbranceData>, RefRO<DodgeData>, RefRO<FirstPersonCameraData>,
                         RefRW<LocalTransform>>())
            {
                // Update movement state based on input
                UpdateMovementState(ref state.ValueRW, in input.ValueRO, in stamina.ValueRO);

                // Calculate target speed based on current state
                float targetSpeed = GetTargetSpeed(in state.ValueRO, in movement.ValueRO, in encumbrance.ValueRO);
                state.ValueRW.CurrentSpeed = targetSpeed;

                // Handle dodge movement (overrides normal movement)
                if (dodge.ValueRO.IsDodging)
                {
                    ApplyDodgeMovement(ref movement.ValueRW, ref transform.ValueRW, in dodge.ValueRO,
                        in camera.ValueRO, deltaTime);
                }
                // Normal movement
                else
                {
                    ApplyMovement(ref movement.ValueRW, ref transform.ValueRW, in input.ValueRO,
                        in camera.ValueRO, targetSpeed, deltaTime);
                }

                // Apply gravity
                if (!movement.ValueRO.IsGrounded)
                {
                    movement.ValueRW.Velocity.y -= movement.ValueRO.Gravity * deltaTime;
                }
                else
                {
                    movement.ValueRW.Velocity.y = -2f; // Small downward force to keep grounded
                }

                // Apply velocity to position
                transform.ValueRW.Position += movement.ValueRO.Velocity * deltaTime;
            }
        }

        /// <summary>
        /// Updates the character's movement state based on input
        /// </summary>
        private void UpdateMovementState(ref CharacterStateData state, in PlayerInputData input, in StaminaData stamina)
        {
            state.PreviousState = state.CurrentState;

            // Priority order: Prone > Crouch > Sprint > Walk > Idle

            // Check for prone (lowest stance)
            if (input.PronePressed)
            {
                state.CurrentState = MovementState.Prone;
                return;
            }

            // Check for crouch
            if (input.CrouchPressed || input.IsCrouchToggled)
            {
                state.CurrentState = MovementState.Crouching;
                return;
            }

            // Check if player is trying to move
            bool isMoving = math.lengthsq(input.MoveInput) > 0.01f;

            if (isMoving)
            {
                // Check for sprint (requires stamina and sprint input)
                if ((input.SprintPressed || input.IsSprintToggled) && !stamina.IsExhausted)
                {
                    state.CurrentState = MovementState.Sprinting;
                }
                else
                {
                    state.CurrentState = MovementState.Walking;
                }
            }
            else
            {
                state.CurrentState = MovementState.Idle;
            }
        }

        /// <summary>
        /// Gets the target movement speed based on current state and modifiers
        /// </summary>
        private float GetTargetSpeed(in CharacterStateData state, in CharacterMovementData movement,
            in EncumbranceData encumbrance)
        {
            float baseSpeed = state.CurrentState switch
            {
                MovementState.Sprinting => movement.SprintSpeed,
                MovementState.Walking => movement.WalkSpeed,
                MovementState.Crouching => movement.CrouchSpeed,
                MovementState.Prone => movement.ProneSpeed,
                _ => 0f // Idle
            };

            // Apply encumbrance penalty
            return baseSpeed * encumbrance.MovementSpeedMultiplier;
        }

        /// <summary>
        /// Applies normal WASD movement
        /// </summary>
        private void ApplyMovement(ref CharacterMovementData movement, ref LocalTransform transform,
            in PlayerInputData input, in FirstPersonCameraData camera, float targetSpeed, float deltaTime)
        {
            if (math.lengthsq(input.MoveInput) < 0.01f)
            {
                // No input - decelerate
                movement.Velocity.x = math.lerp(movement.Velocity.x, 0f, movement.Deceleration * deltaTime);
                movement.Velocity.z = math.lerp(movement.Velocity.z, 0f, movement.Deceleration * deltaTime);
                return;
            }

            // Calculate movement direction relative to camera yaw
            float yawRadians = math.radians(camera.Yaw);
            float3 forward = new float3(math.sin(yawRadians), 0f, math.cos(yawRadians));
            float3 right = new float3(math.cos(yawRadians), 0f, -math.sin(yawRadians));

            // Calculate desired movement direction
            float3 moveDirection = (forward * input.MoveInput.y + right * input.MoveInput.x);
            moveDirection = math.normalizesafe(moveDirection);

            // Calculate target velocity
            float3 targetVelocity = moveDirection * targetSpeed;

            // Accelerate towards target velocity (only XZ plane)
            float accel = movement.IsGrounded ? movement.Acceleration : movement.AirControl;

            movement.Velocity.x = math.lerp(movement.Velocity.x, targetVelocity.x, accel * deltaTime);
            movement.Velocity.z = math.lerp(movement.Velocity.z, targetVelocity.z, accel * deltaTime);
        }

        /// <summary>
        /// Applies dodge movement (2.5m displacement in strafe direction)
        /// </summary>
        private void ApplyDodgeMovement(ref CharacterMovementData movement, ref LocalTransform transform,
            in DodgeData dodge, in FirstPersonCameraData camera, float deltaTime)
        {
            // Calculate dodge progress (0 to 1)
            float dodgeProgress = dodge.DodgeTimer / dodge.DodgeDuration;

            // Use smoothstep for smooth acceleration/deceleration
            float smoothProgress = dodgeProgress * dodgeProgress * (3f - 2f * dodgeProgress);

            // Calculate dodge speed (distance / duration, modulated by curve)
            float dodgeSpeed = (dodge.DodgeDistance / dodge.DodgeDuration);

            // Calculate dodge direction (perpendicular to forward, based on strafe input)
            float yawRadians = math.radians(camera.Yaw);
            float3 dodgeDirection = new float3(
                math.cos(yawRadians) * dodge.DodgeDirectionX,
                0f,
                -math.sin(yawRadians) * dodge.DodgeDirectionX
            );

            // Apply dodge velocity (overrides normal movement)
            float speedMultiplier = 1f - math.abs(smoothProgress - 0.5f) * 2f; // Peak speed at middle
            movement.Velocity.x = dodgeDirection.x * dodgeSpeed * speedMultiplier;
            movement.Velocity.z = dodgeDirection.z * dodgeSpeed * speedMultiplier;
        }
    }
}
