using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Ground detection data for character controller
    /// Uses raycasting to detect ground and slopes
    /// Based on GDD.md movement requirements
    /// </summary>
    public struct GroundDetectionData : IComponentData
    {
        // Ground check parameters
        public float GroundCheckDistance;    // 0.1m default
        public float GroundCheckRadius;      // Slightly smaller than capsule radius
        public float3 GroundCheckOffset;     // Offset from character center

        // Ground state
        public bool IsGrounded;
        public bool WasGroundedLastFrame;
        public float TimeSinceGrounded;      // Time since last grounded
        public float3 GroundNormal;          // Normal of ground surface
        public float GroundAngle;            // Angle of ground slope

        // Landed detection
        public bool JustLanded;              // True on the frame we land

        // Surface properties
        public float GroundDistance;         // Distance to ground
        public Entity GroundEntity;          // Entity we're standing on (if any)
    }
}
