using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// ECS system that calculates camera rotation, FOV, and effects data
    /// This system handles the data/logic side, while CinemachineCameraController
    /// (MonoBehaviour) applies the values to the actual Cinemachine camera
    ///
    /// This bridge pattern allows us to keep ECS performance while using
    /// Cinemachine's powerful camera features (damping, shake, noise, etc.)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CharacterMovementSystem))]
    public partial class CinemachineCameraBridgeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (cameraData, inputData, transform, state, encumbrance, entity) in
                     SystemAPI.Query<RefRW<CinemachineCameraData>,
                                    RefRO<PlayerInputData>,
                                    RefRW<LocalTransform>,
                                    RefRO<CharacterStateData>,
                                    RefRO<EncumbranceData>>()
                     .WithEntityAccess())
            {
                // ===== MOUSE INPUT & ROTATION =====
                ApplyMouseInput(ref cameraData.ValueRW, inputData.ValueRO);

                // ===== WEIGHT-BASED EFFECTS =====
                UpdateWeightEffects(ref cameraData.ValueRW, encumbrance.ValueRO);

                // ===== FOV CHANGES =====
                UpdateFOV(ref cameraData.ValueRW, state.ValueRO, deltaTime);

                // ===== BREATHING & IDLE SWAY =====
                if (cameraData.ValueRO.EnableProceduralEffects)
                {
                    UpdateBreathing(ref cameraData.ValueRW, state.ValueRO, deltaTime);
                }

                // ===== RECOIL PROCESSING =====
                ProcessRecoil(ref cameraData.ValueRW, entity, deltaTime);

                // ===== CAMERA SHAKE REQUESTS =====
                ProcessShakeRequests(ref cameraData.ValueRW, entity);

                // ===== LANDING DETECTION =====
                ProcessLandingEffects(ref cameraData.ValueRW, entity);

                // Apply yaw rotation to character body (horizontal look)
                quaternion yawRotation = quaternion.RotateY(math.radians(cameraData.ValueRO.Yaw));
                transform.ValueRW.Rotation = yawRotation;
            }
        }

        /// <summary>
        /// Apply mouse input to camera rotation with sensitivity
        /// </summary>
        private void ApplyMouseInput(ref CinemachineCameraData cameraData, PlayerInputData inputData)
        {
            // Apply mouse input to camera rotation
            cameraData.Yaw += inputData.LookInput.x * cameraData.MouseSensitivityX;
            cameraData.Pitch -= inputData.LookInput.y * cameraData.MouseSensitivityY;

            // Clamp pitch to prevent over-rotation
            cameraData.Pitch = math.clamp(
                cameraData.Pitch,
                cameraData.MinPitch,
                cameraData.MaxPitch
            );

            // Wrap yaw to 0-360 range
            if (cameraData.Yaw >= 360f)
                cameraData.Yaw -= 360f;
            else if (cameraData.Yaw < 0f)
                cameraData.Yaw += 360f;
        }

        /// <summary>
        /// Calculate weight-based camera damping and shake
        /// More weight = more camera lag and shake
        /// </summary>
        private void UpdateWeightEffects(ref CinemachineCameraData cameraData, EncumbranceData encumbrance)
        {
            // Calculate weight ratio (0.0 = no weight, 1.0 = max weight)
            float weightRatio = encumbrance.CurrentWeight / encumbrance.AbsoluteMaxWeight;

            // Increase camera damping based on weight (heavier = more sluggish camera)
            // Light: 0.1-0.3, Medium: 0.3-0.6, Heavy: 0.6-1.5
            float weightDampingMultiplier = math.lerp(1.0f, cameraData.EncumberedDampingMultiplier, weightRatio);
            cameraData.CurrentRotationDamping = cameraData.BaseRotationDamping * weightDampingMultiplier;

            // Increase shake when overencumbered (>75% capacity)
            if (weightRatio > 0.75f)
            {
                float overencumberedRatio = (weightRatio - 0.75f) / 0.25f; // 0.0-1.0 range
                // Apply extra shake multiplier
                // This will be used by the MonoBehaviour to increase noise amplitude
            }
        }

        /// <summary>
        /// Update FOV based on movement state (sprint = wider FOV)
        /// </summary>
        private void UpdateFOV(ref CinemachineCameraData cameraData, CharacterStateData state, float deltaTime)
        {
            // Target FOV based on movement state
            float targetFOV = state.CurrentState == MovementState.Sprinting
                ? cameraData.SprintFOV
                : cameraData.BaseFOV;

            // ADS overrides other FOV changes (would be set by weapon system)
            if (cameraData.IsADS)
            {
                // ADS FOV would be set by weapon system based on scope magnification
                // For now, just reduce by 30%
                targetFOV = cameraData.BaseFOV * 0.7f;
            }

            // Smooth lerp to target FOV
            cameraData.CurrentFOV = math.lerp(
                cameraData.CurrentFOV,
                targetFOV,
                deltaTime * cameraData.FOVLerpSpeed
            );
        }

        /// <summary>
        /// Update breathing cycle and idle sway
        /// Creates subtle camera movement for realism
        /// </summary>
        private void UpdateBreathing(ref CinemachineCameraData cameraData, CharacterStateData state, float deltaTime)
        {
            // Update breathing phase
            cameraData.CurrentBreathPhase += deltaTime * cameraData.BreathingFrequency;
            if (cameraData.CurrentBreathPhase > math.PI * 2f)
                cameraData.CurrentBreathPhase -= math.PI * 2f;

            // Breathing amplitude varies by stance
            float breathingMultiplier = 1.0f;
            switch (state.CurrentState)
            {
                case MovementState.Prone:
                    breathingMultiplier = cameraData.ProneStabilization; // Very stable
                    break;
                case MovementState.Crouching:
                    breathingMultiplier = cameraData.CrouchStabilization; // Fairly stable
                    break;
                case MovementState.Sprinting:
                    breathingMultiplier = 2.0f; // Heavy breathing
                    break;
                case MovementState.Idle:
                    breathingMultiplier = 0.5f; // Minimal
                    break;
            }

            // ADS reduces breathing
            if (cameraData.IsADS)
            {
                breathingMultiplier *= cameraData.ADSStabilization;
            }

            // Breathing amplitude will be used by MonoBehaviour for procedural noise
            // The MonoBehaviour reads CurrentBreathPhase and BreathingAmplitude
        }

        /// <summary>
        /// Process recoil requests from weapon system
        /// Applies camera kick and gradual recovery
        /// </summary>
        private void ProcessRecoil(ref CinemachineCameraData cameraData, Entity entity, float deltaTime)
        {
            // Check for recoil requests
            if (SystemAPI.HasComponent<CameraRecoilRequest>(entity))
            {
                var recoilRequest = SystemAPI.GetComponent<CameraRecoilRequest>(entity);

                // Add recoil to current amounts (accumulates)
                cameraData.RecoilPitchAmount += recoilRequest.PitchRecoil;
                cameraData.RecoilYawAmount += recoilRequest.YawRecoil;

                // Remove the request component
                EntityManager.RemoveComponent<CameraRecoilRequest>(entity);
            }

            // Gradually recover from recoil
            if (math.abs(cameraData.RecoilPitchAmount) > 0.01f || math.abs(cameraData.RecoilYawAmount) > 0.01f)
            {
                float recoveryAmount = deltaTime * cameraData.RecoilRecoverySpeed;

                cameraData.RecoilPitchAmount = math.lerp(cameraData.RecoilPitchAmount, 0f, recoveryAmount);
                cameraData.RecoilYawAmount = math.lerp(cameraData.RecoilYawAmount, 0f, recoveryAmount);

                // Apply recoil to camera pitch (yaw recoil is applied as shake)
                cameraData.Pitch += cameraData.RecoilPitchAmount * deltaTime * 10f; // Scaled for smooth kick

                // Clamp after recoil
                cameraData.Pitch = math.clamp(cameraData.Pitch, cameraData.MinPitch, cameraData.MaxPitch);
            }
        }

        /// <summary>
        /// Process camera shake requests from various systems
        /// (weapon fire, explosions, impacts, etc.)
        /// </summary>
        private void ProcessShakeRequests(ref CinemachineCameraData cameraData, Entity entity)
        {
            // Check for shake requests
            if (SystemAPI.HasComponent<CameraShakeRequest>(entity))
            {
                var shakeRequest = SystemAPI.GetComponent<CameraShakeRequest>(entity);

                // The MonoBehaviour will read this and trigger Cinemachine Impulse
                // We just need to mark that a shake is requested
                // (MonoBehaviour polls for new shake requests each frame)

                // Note: The actual shake is applied by CinemachineCameraController
                // which reads CameraShakeRequest and triggers Cinemachine Impulse Source

                // Remove the request after one frame
                EntityManager.RemoveComponent<CameraShakeRequest>(entity);
            }
        }

        /// <summary>
        /// Detect landing events and trigger landing camera shake
        /// </summary>
        private void ProcessLandingEffects(ref CinemachineCameraData cameraData, Entity entity)
        {
            // Check if GroundDetectionData exists
            if (SystemAPI.HasComponent<GroundDetectionData>(entity))
            {
                var groundData = SystemAPI.GetComponent<GroundDetectionData>(entity);

                // Landing detected
                if (groundData.JustLanded)
                {
                    cameraData.TriggerLandingShake = true;

                    // Calculate landing intensity based on fall time
                    float fallTime = groundData.TimeSinceGrounded;
                    float intensity = math.clamp(fallTime / 1.0f, 0.1f, 1.0f); // Scale 0-1 second fall

                    // Create shake request for landing
                    float shakeAmplitude = cameraData.LandingImpactStrength * intensity;

                    // Add shake request component
                    EntityManager.AddComponentData(entity, new CameraShakeRequest
                    {
                        Amplitude = shakeAmplitude,
                        Frequency = 15f, // Sharp shake
                        Duration = 0.2f,
                        Direction = new float3(0, -1, 0) // Downward impact
                    });
                }
            }
        }
    }
}
