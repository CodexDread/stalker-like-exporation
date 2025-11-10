using Unity.Entities;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Dodge system data
    /// Based on GDD.md Combat Mechanics - Dodge System
    ///
    /// Activation: Jump while strafing (A or D keys)
    /// Effect: 2.5m quick displacement in strafe direction
    /// Stamina Cost: 15 points
    /// Cooldown: 1.5 seconds
    /// Benefits: 0.2s i-frames, breaks mutant target lock
    /// Limitations: Cannot dodge when overencumbered (>45kg), no dodging while prone
    /// </summary>
    public struct DodgeData : IComponentData
    {
        // Dodge parameters
        public float DodgeDistance;        // 2.5m as per GDD
        public float DodgeDuration;        // Time to complete dodge movement
        public float IFrameDuration;       // 0.2s invulnerability frames
        public float StaminaCost;          // 15 stamina points
        public float CooldownTime;         // 1.5s between dodges

        // Current state
        public bool IsDodging;
        public bool HasIFrames;
        public float DodgeTimer;           // Current dodge progress
        public float CooldownTimer;        // Current cooldown remaining
        public float IFrameTimer;          // Current i-frame time remaining

        // Dodge direction (set when dodge is triggered)
        public float DodgeDirectionX;      // -1 for left, +1 for right
    }
}
