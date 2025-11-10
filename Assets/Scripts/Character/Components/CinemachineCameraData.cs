using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Cinemachine-based first-person camera data
    /// Provides rotation, FOV, and procedural effects for enhanced camera feel
    /// Integrates with Cinemachine for weight, inertia, and camera shake
    /// Based on GDD.md UI/UX Design and Controls
    /// </summary>
    public struct CinemachineCameraData : IComponentData
    {
        // Mouse sensitivity
        public float MouseSensitivityX;
        public float MouseSensitivityY;

        // Rotation limits
        public float MinPitch; // Look down limit (typically -85)
        public float MaxPitch; // Look up limit (typically +85)

        // Current rotation (calculated by ECS, applied to Cinemachine)
        public float Pitch;    // Up/down rotation (X-axis)
        public float Yaw;      // Left/right rotation (Y-axis)

        // Camera offset from character
        public float3 CameraOffset; // Height offset for camera position

        // FOV (Field of View)
        public float BaseFOV;       // Normal FOV
        public float SprintFOV;     // FOV when sprinting (slight increase for speed feel)
        public float CurrentFOV;    // Current interpolated FOV
        public float FOVLerpSpeed;  // How fast FOV transitions

        // Weight & Inertia Effects
        public float BaseRotationDamping;     // Base camera rotation smoothing (0.1-2.0)
        public float EncumberedDampingMultiplier; // How much extra damping when carrying weight
        public float CurrentRotationDamping;  // Calculated damping based on weight

        public float BaseFOVShake;            // Idle camera sway amount
        public float MovementShakeMultiplier; // Shake multiplier when moving
        public float EncumberedShakeMultiplier; // Extra shake when overencumbered

        // Landing/Impact Effects
        public float LandingImpactStrength;   // Camera shake on landing
        public float LastLandingTime;         // Track landing events
        public bool TriggerLandingShake;      // Request landing shake

        // Recoil Effects
        public float RecoilPitchAmount;      // Current recoil pitch offset
        public float RecoilYawAmount;        // Current recoil yaw offset
        public float RecoilRecoverySpeed;    // How fast recoil returns to center

        // Breathing & Idle Sway
        public float BreathingFrequency;     // Breathing cycle frequency
        public float BreathingAmplitude;     // Breathing sway amount
        public float IdleSwayAmount;         // Random idle sway
        public float CurrentBreathPhase;     // Current breathing animation phase

        // Stance-based effects
        public float ProneStabilization;     // Reduced shake when prone (0.2x)
        public float CrouchStabilization;    // Reduced shake when crouched (0.5x)
        public float ADSStabilization;       // Reduced shake when aiming down sights (0.3x)
        public bool IsADS;                   // Currently aiming down sights

        // Performance
        public bool EnableProceduralEffects; // Master toggle for all procedural camera effects
        public float EffectsIntensity;       // Global intensity multiplier (0.0-1.0)
    }

    /// <summary>
    /// Tag to identify the entity that owns the Cinemachine virtual camera GameObject
    /// Used to bridge ECS data to Cinemachine MonoBehaviour
    /// </summary>
    public struct CinemachineCameraOwnerTag : IComponentData
    {
        public int CameraInstanceID; // InstanceID of the Cinemachine virtual camera
    }

    /// <summary>
    /// Request component for triggering camera shake effects
    /// Systems add this to request shake, CinemachineBridge processes and removes it
    /// </summary>
    public struct CameraShakeRequest : IComponentData
    {
        public float Amplitude;   // Shake strength
        public float Frequency;   // Shake speed
        public float Duration;    // How long the shake lasts
        public float3 Direction;  // Directional shake (for impacts)
    }

    /// <summary>
    /// Request component for camera recoil
    /// Weapon firing system adds this, camera applies the recoil
    /// </summary>
    public struct CameraRecoilRequest : IComponentData
    {
        public float PitchRecoil;  // Upward kick
        public float YawRecoil;    // Horizontal kick (randomized left/right)
        public float RecoveryTime; // How long to return to center
    }
}
