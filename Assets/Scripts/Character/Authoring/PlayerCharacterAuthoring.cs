using Unity.Entities;
using UnityEngine;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Authoring component to convert GameObject to ECS entity
    /// Attach this to a GameObject to create a player character entity
    /// Based on GDD.md character and movement specifications
    /// </summary>
    public class PlayerCharacterAuthoring : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Walking speed in m/s")]
        public float walkSpeed = 3.5f;

        [Tooltip("Sprinting speed in m/s")]
        public float sprintSpeed = 6.5f;

        [Tooltip("Crouching speed in m/s")]
        public float crouchSpeed = 1.8f;

        [Tooltip("Prone movement speed in m/s")]
        public float proneSpeed = 0.8f;

        [Tooltip("Movement acceleration (higher = more responsive)")]
        public float acceleration = 10f;

        [Tooltip("Movement deceleration (higher = stops faster)")]
        public float deceleration = 15f;

        [Tooltip("Air control multiplier (1.0 = full control, 0.0 = no control)")]
        public float airControl = 0.3f;

        [Header("Jump & Gravity")]
        [Tooltip("Jump force")]
        public float jumpForce = 5f;

        [Tooltip("Gravity strength")]
        public float gravity = 20f;

        [Tooltip("Ground check distance")]
        public float groundCheckDistance = 0.1f;

        [Header("Stamina System")]
        [Tooltip("Maximum stamina points")]
        public float maxStamina = 100f;

        [Tooltip("Stamina regeneration per second")]
        public float staminaRegenRate = 10f;

        [Tooltip("Stamina drain per second while sprinting")]
        public float sprintDrainRate = 15f;

        [Tooltip("Minimum stamina required to resume sprinting after exhaustion")]
        public float exhaustionRecoveryThreshold = 30f;

        [Header("Dodge System (GDD: 2.5m, 15 stamina, 1.5s cooldown, 0.2s i-frames)")]
        [Tooltip("Distance traveled during dodge (GDD: 2.5m)")]
        public float dodgeDistance = 2.5f;

        [Tooltip("Duration of dodge animation/movement")]
        public float dodgeDuration = 0.3f;

        [Tooltip("Invulnerability frame duration (GDD: 0.2s)")]
        public float iFrameDuration = 0.2f;

        [Tooltip("Stamina cost per dodge (GDD: 15 points)")]
        public float dodgeStaminaCost = 15f;

        [Tooltip("Cooldown between dodges (GDD: 1.5s)")]
        public float dodgeCooldown = 1.5f;

        [Header("Encumbrance (GDD: 30kg base, 60kg max, 45kg dodge limit)")]
        [Tooltip("Base maximum carry weight in kg (GDD: 30kg)")]
        public float baseMaxWeight = 30f;

        [Tooltip("Absolute maximum weight in kg (GDD: 60kg)")]
        public float absoluteMaxWeight = 60f;

        [Tooltip("Maximum weight to allow dodging (GDD: 45kg)")]
        public float dodgeWeightLimit = 45f;

        [Tooltip("Current carried weight (set this to test encumbrance)")]
        public float currentWeight = 10f;

        [Header("Physics & Collision")]
        [Tooltip("Character mass in kg")]
        public float mass = 75f;

        [Tooltip("Air drag/resistance")]
        public float drag = 0.1f;

        [Tooltip("Maximum step height character can climb")]
        public float stepHeight = 0.3f;

        [Tooltip("Maximum walkable slope angle in degrees")]
        public float slopeLimit = 45f;

        [Header("Capsule Heights")]
        [Tooltip("Capsule height when standing (1.8m)")]
        public float standingHeight = 1.8f;

        [Tooltip("Capsule height when crouching (1.2m)")]
        public float crouchingHeight = 1.2f;

        [Tooltip("Capsule height when prone (0.5m)")]
        public float proneHeight = 0.5f;

        [Tooltip("Capsule radius")]
        public float capsuleRadius = 0.3f;

        [Tooltip("Height transition speed (higher = faster stance changes)")]
        public float heightTransitionSpeed = 8f;

        [Header("Ground Detection")]
        [Tooltip("Ground check radius (slightly smaller than capsule)")]
        public float groundCheckRadius = 0.25f;

        [Tooltip("Offset for ground check ray")]
        public Vector3 groundCheckOffset = new Vector3(0f, 0f, 0f);

        [Header("Jump Settings")]
        [Tooltip("Time to buffer jump input before landing (0.15s)")]
        public float jumpBufferTime = 0.15f;

        [Tooltip("Coyote time - grace period after leaving ground (0.1s)")]
        public float coyoteTime = 0.1f;

        [Tooltip("Cooldown between jumps (0.2s)")]
        public float jumpCooldown = 0.2f;

        [Tooltip("Stamina cost per jump (optional, 0 = no cost)")]
        public float jumpStaminaCost = 10f;

        [Header("Camera Settings")]
        [Tooltip("Mouse horizontal sensitivity")]
        public float mouseSensitivityX = 2f;

        [Tooltip("Mouse vertical sensitivity")]
        public float mouseSensitivityY = 2f;

        [Tooltip("Minimum pitch angle (look down limit)")]
        public float minPitch = -85f;

        [Tooltip("Maximum pitch angle (look up limit)")]
        public float maxPitch = 85f;

        [Tooltip("Camera height offset from character position")]
        public Vector3 cameraOffset = new Vector3(0f, 1.6f, 0f);

        [Tooltip("Normal field of view")]
        public float baseFOV = 75f;

        [Tooltip("Field of view when sprinting")]
        public float sprintFOV = 85f;

        [Tooltip("FOV transition speed")]
        public float fovLerpSpeed = 5f;

        class Baker : Baker<PlayerCharacterAuthoring>
        {
            public override void Bake(PlayerCharacterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add movement data
                AddComponent(entity, new CharacterMovementData
                {
                    WalkSpeed = authoring.walkSpeed,
                    SprintSpeed = authoring.sprintSpeed,
                    CrouchSpeed = authoring.crouchSpeed,
                    ProneSpeed = authoring.proneSpeed,
                    Acceleration = authoring.acceleration,
                    Deceleration = authoring.deceleration,
                    AirControl = authoring.airControl,
                    JumpForce = authoring.jumpForce,
                    Gravity = authoring.gravity,
                    GroundCheckDistance = authoring.groundCheckDistance,
                    IsGrounded = true // Start on ground
                });

                // Add state data
                AddComponent(entity, new CharacterStateData
                {
                    CurrentState = MovementState.Idle,
                    PreviousState = MovementState.Idle,
                    CurrentSpeed = 0f,
                    CurrentHeight = 1.8f // Standing height
                });

                // Add stamina data
                AddComponent(entity, new StaminaData
                {
                    Current = authoring.maxStamina,
                    Maximum = authoring.maxStamina,
                    RegenRate = authoring.staminaRegenRate,
                    SprintDrainRate = authoring.sprintDrainRate,
                    DodgeCost = authoring.dodgeStaminaCost,
                    IsExhausted = false,
                    ExhaustedRecoveryThreshold = authoring.exhaustionRecoveryThreshold,
                    StaminaMultiplier = 1.0f // Modified by skills
                });

                // Add dodge data
                AddComponent(entity, new DodgeData
                {
                    DodgeDistance = authoring.dodgeDistance,
                    DodgeDuration = authoring.dodgeDuration,
                    IFrameDuration = authoring.iFrameDuration,
                    StaminaCost = authoring.dodgeStaminaCost,
                    CooldownTime = authoring.dodgeCooldown,
                    IsDodging = false,
                    HasIFrames = false
                });

                // Add encumbrance data
                AddComponent(entity, new EncumbranceData
                {
                    CurrentWeight = authoring.currentWeight,
                    BaseMaxWeight = authoring.baseMaxWeight,
                    AbsoluteMaxWeight = authoring.absoluteMaxWeight,
                    DodgeWeightLimit = authoring.dodgeWeightLimit,
                    SkillWeightBonus = 0f, // Modified by skills
                    MovementSpeedMultiplier = 1.0f
                });

                // Add camera data
                AddComponent(entity, new FirstPersonCameraData
                {
                    MouseSensitivityX = authoring.mouseSensitivityX,
                    MouseSensitivityY = authoring.mouseSensitivityY,
                    MinPitch = authoring.minPitch,
                    MaxPitch = authoring.maxPitch,
                    Pitch = 0f,
                    Yaw = 0f,
                    CameraOffset = authoring.cameraOffset,
                    BaseFOV = authoring.baseFOV,
                    SprintFOV = authoring.sprintFOV,
                    CurrentFOV = authoring.baseFOV,
                    FOVLerpSpeed = authoring.fovLerpSpeed
                });

                // Add input data (initialized empty)
                AddComponent(entity, new PlayerInputData());

                // Add physics data
                AddComponent(entity, new CharacterPhysicsData
                {
                    StandingHeight = authoring.standingHeight,
                    CrouchingHeight = authoring.crouchingHeight,
                    ProneHeight = authoring.proneHeight,
                    CapsuleRadius = authoring.capsuleRadius,
                    CurrentHeight = authoring.standingHeight,
                    TargetHeight = authoring.standingHeight,
                    HeightTransitionSpeed = authoring.heightTransitionSpeed,
                    Mass = authoring.mass,
                    Drag = authoring.drag,
                    StepHeight = authoring.stepHeight,
                    SlopeLimit = authoring.slopeLimit,
                    CollisionLayer = 1u, // Default player layer
                    CollisionMask = ~0u  // Collide with everything
                });

                // Add ground detection data
                AddComponent(entity, new GroundDetectionData
                {
                    GroundCheckDistance = authoring.groundCheckDistance,
                    GroundCheckRadius = authoring.groundCheckRadius,
                    GroundCheckOffset = authoring.groundCheckOffset,
                    IsGrounded = true,
                    WasGroundedLastFrame = true,
                    GroundNormal = new float3(0, 1, 0)
                });

                // Add jump data
                AddComponent(entity, new JumpData
                {
                    JumpForce = authoring.jumpForce,
                    JumpCooldown = authoring.jumpCooldown,
                    JumpBufferTime = authoring.jumpBufferTime,
                    CoyoteTime = authoring.coyoteTime,
                    JumpStaminaCost = authoring.jumpStaminaCost,
                    CanJump = true,
                    JumpsRemaining = 1
                });
            }
        }
    }
}
