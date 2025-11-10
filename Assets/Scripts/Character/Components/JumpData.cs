using Unity.Entities;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Jump mechanics data
    /// Handles jump input, coyote time, and jump buffering
    /// Based on GDD.md movement specifications
    /// </summary>
    public struct JumpData : IComponentData
    {
        // Jump parameters
        public float JumpForce;              // Upward force applied on jump
        public float JumpCooldown;           // Time between jumps (0.2s default)

        // Jump buffering (press jump slightly before landing)
        public float JumpBufferTime;         // How long to buffer jump input (0.15s)
        public float JumpBufferCounter;      // Current buffer time remaining

        // Coyote time (grace period after leaving ground)
        public float CoyoteTime;             // 0.1s default
        public float CoyoteTimeCounter;      // Current coyote time remaining

        // Jump state
        public bool IsJumping;               // Currently in air from jump
        public bool CanJump;                 // Can execute jump now
        public float JumpCooldownCounter;    // Current cooldown remaining
        public int JumpsRemaining;           // For potential double-jump (set to 1 for single jump)

        // Stamina cost (optional - not in GDD but makes sense)
        public float JumpStaminaCost;        // 10 stamina per jump
    }
}
