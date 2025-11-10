using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Physics configuration for character controller
    /// Handles capsule dimensions, collision, and physics properties
    /// </summary>
    public struct CharacterPhysicsData : IComponentData
    {
        // Capsule dimensions for different stances
        public float StandingHeight;     // 1.8m for standing
        public float CrouchingHeight;    // 1.2m for crouching
        public float ProneHeight;        // 0.5m for prone
        public float CapsuleRadius;      // 0.3m default

        // Current capsule state
        public float CurrentHeight;
        public float TargetHeight;
        public float HeightTransitionSpeed; // How fast to transition between heights

        // Physics properties
        public float Mass;               // Character mass in kg
        public float Drag;               // Air resistance
        public float StepHeight;         // Maximum step height (0.3m default)
        public float SlopeLimit;         // Maximum walkable slope angle (45° default)

        // Collision layers
        public uint CollisionLayer;      // What layer this character is on
        public uint CollisionMask;       // What layers this character collides with
    }
}
