using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Core movement data for the character controller
    /// Based on GDD.md movement specifications
    /// </summary>
    public struct CharacterMovementData : IComponentData
    {
        // Movement speeds (m/s)
        public float WalkSpeed;
        public float SprintSpeed;
        public float CrouchSpeed;
        public float ProneSpeed;

        // Acceleration
        public float Acceleration;
        public float Deceleration;
        public float AirControl;

        // Jump
        public float JumpForce;
        public float Gravity;

        // Current velocity
        public float3 Velocity;

        // Ground detection
        public bool IsGrounded;
        public float GroundCheckDistance;
    }
}
