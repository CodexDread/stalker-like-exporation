using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Authoring component for Cinemachine-based first-person camera
    /// Attach this to the player character GameObject to set up the camera system
    /// This creates both the ECS data and the Cinemachine GameObject hierarchy
    /// </summary>
    public class CinemachineCameraAuthoring : MonoBehaviour
    {
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

        [Header("Field of View")]
        [Tooltip("Normal field of view")]
        public float baseFOV = 75f;

        [Tooltip("Field of view when sprinting")]
        public float sprintFOV = 85f;

        [Tooltip("FOV transition speed")]
        public float fovLerpSpeed = 5f;

        [Header("Weight & Inertia Effects")]
        [Tooltip("Base camera rotation damping (0.1 = very responsive, 2.0 = very sluggish)")]
        [Range(0.1f, 3f)]
        public float baseRotationDamping = 0.3f;

        [Tooltip("Damping multiplier when fully encumbered (1.0 = no change, 3.0 = 3x slower)")]
        [Range(1f, 5f)]
        public float encumberedDampingMultiplier = 2.5f;

        [Header("Procedural Camera Effects")]
        [Tooltip("Enable all procedural camera effects (breathing, idle sway, shake)")]
        public bool enableProceduralEffects = true;

        [Tooltip("Global intensity multiplier for all effects (0 = off, 1 = full)")]
        [Range(0f, 1f)]
        public float effectsIntensity = 0.7f;

        [Header("Breathing & Idle Sway")]
        [Tooltip("Breathing cycle frequency (breaths per second)")]
        [Range(0.1f, 1f)]
        public float breathingFrequency = 0.25f; // ~15 breaths per minute

        [Tooltip("Breathing sway amplitude")]
        [Range(0f, 0.5f)]
        public float breathingAmplitude = 0.05f;

        [Tooltip("Random idle camera sway amount")]
        [Range(0f, 0.3f)]
        public float idleSwayAmount = 0.02f;

        [Tooltip("Shake multiplier when moving")]
        [Range(1f, 3f)]
        public float movementShakeMultiplier = 1.5f;

        [Tooltip("Extra shake when overencumbered")]
        [Range(1f, 4f)]
        public float encumberedShakeMultiplier = 2.0f;

        [Header("Impact & Landing Effects")]
        [Tooltip("Camera shake strength on landing")]
        [Range(0f, 2f)]
        public float landingImpactStrength = 0.5f;

        [Header("Recoil Settings")]
        [Tooltip("Recoil recovery speed (higher = faster return to center)")]
        [Range(1f, 20f)]
        public float recoilRecoverySpeed = 8f;

        [Header("Stance-Based Stabilization")]
        [Tooltip("Camera shake multiplier when prone (0.2 = 80% reduction)")]
        [Range(0.1f, 1f)]
        public float proneStabilization = 0.2f;

        [Tooltip("Camera shake multiplier when crouched (0.5 = 50% reduction)")]
        [Range(0.1f, 1f)]
        public float crouchStabilization = 0.5f;

        [Tooltip("Camera shake multiplier when aiming down sights (0.3 = 70% reduction)")]
        [Range(0.1f, 1f)]
        public float adsStabilization = 0.3f;

        class Baker : Baker<CinemachineCameraAuthoring>
        {
            public override void Bake(CinemachineCameraAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add Cinemachine camera component with all settings
                AddComponent(entity, new CinemachineCameraData
                {
                    // Mouse & Rotation
                    MouseSensitivityX = authoring.mouseSensitivityX,
                    MouseSensitivityY = authoring.mouseSensitivityY,
                    MinPitch = authoring.minPitch,
                    MaxPitch = authoring.maxPitch,
                    Pitch = 0f,
                    Yaw = 0f,

                    // Camera Offset
                    CameraOffset = new float3(
                        authoring.cameraOffset.x,
                        authoring.cameraOffset.y,
                        authoring.cameraOffset.z
                    ),

                    // FOV
                    BaseFOV = authoring.baseFOV,
                    SprintFOV = authoring.sprintFOV,
                    CurrentFOV = authoring.baseFOV,
                    FOVLerpSpeed = authoring.fovLerpSpeed,

                    // Weight & Inertia
                    BaseRotationDamping = authoring.baseRotationDamping,
                    EncumberedDampingMultiplier = authoring.encumberedDampingMultiplier,
                    CurrentRotationDamping = authoring.baseRotationDamping,

                    // Shake Effects
                    BaseFOVShake = authoring.idleSwayAmount,
                    MovementShakeMultiplier = authoring.movementShakeMultiplier,
                    EncumberedShakeMultiplier = authoring.encumberedShakeMultiplier,

                    // Landing
                    LandingImpactStrength = authoring.landingImpactStrength,
                    LastLandingTime = 0f,
                    TriggerLandingShake = false,

                    // Recoil
                    RecoilPitchAmount = 0f,
                    RecoilYawAmount = 0f,
                    RecoilRecoverySpeed = authoring.recoilRecoverySpeed,

                    // Breathing & Idle
                    BreathingFrequency = authoring.breathingFrequency,
                    BreathingAmplitude = authoring.breathingAmplitude,
                    IdleSwayAmount = authoring.idleSwayAmount,
                    CurrentBreathPhase = 0f,

                    // Stance Stabilization
                    ProneStabilization = authoring.proneStabilization,
                    CrouchStabilization = authoring.crouchStabilization,
                    ADSStabilization = authoring.adsStabilization,
                    IsADS = false,

                    // Performance
                    EnableProceduralEffects = authoring.enableProceduralEffects,
                    EffectsIntensity = authoring.effectsIntensity
                });

                // Add camera owner tag (used to identify this entity owns a camera)
                AddComponent(entity, new CinemachineCameraOwnerTag
                {
                    CameraInstanceID = 0 // Will be set at runtime
                });
            }
        }

        #if UNITY_EDITOR
        // Helper: Validate settings
        void OnValidate()
        {
            // Ensure sensible defaults
            if (mouseSensitivityX < 0.1f) mouseSensitivityX = 0.1f;
            if (mouseSensitivityY < 0.1f) mouseSensitivityY = 0.1f;
            if (baseFOV < 30f) baseFOV = 30f;
            if (baseFOV > 120f) baseFOV = 120f;
            if (sprintFOV < baseFOV) sprintFOV = baseFOV + 5f;

            // Clamp breathing frequency to realistic range
            if (breathingFrequency < 0.1f) breathingFrequency = 0.1f;
            if (breathingFrequency > 1f) breathingFrequency = 1f;
        }

        // Helper: Show gizmo for camera offset in Scene view
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 cameraPos = transform.position + cameraOffset;
            Gizmos.DrawWireSphere(cameraPos, 0.1f);
            Gizmos.DrawLine(transform.position, cameraPos);
        }
        #endif
    }
}
