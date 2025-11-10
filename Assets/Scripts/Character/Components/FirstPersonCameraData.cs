using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// First-person camera controller data
    /// Based on GDD.md UI/UX Design and Controls
    /// Handles camera rotation and look mechanics
    /// </summary>
    public struct FirstPersonCameraData : IComponentData
    {
        // Mouse sensitivity
        public float MouseSensitivityX;
        public float MouseSensitivityY;

        // Rotation limits
        public float MinPitch; // Look down limit (typically -90)
        public float MaxPitch; // Look up limit (typically +90)

        // Current rotation
        public float Pitch;    // Up/down rotation (X-axis)
        public float Yaw;      // Left/right rotation (Y-axis)

        // Camera offset from character
        public float3 CameraOffset; // Height offset for camera position

        // FOV (Field of View)
        public float BaseFOV;       // Normal FOV
        public float SprintFOV;     // FOV when sprinting (slight increase for speed feel)
        public float CurrentFOV;    // Current interpolated FOV
        public float FOVLerpSpeed;  // How fast FOV transitions
    }
}
