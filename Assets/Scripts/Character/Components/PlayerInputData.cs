using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Player input data component
    /// Captures all player inputs for processing by systems
    /// Based on GDD.md Controls section
    /// </summary>
    public struct PlayerInputData : IComponentData
    {
        // Movement input (WASD)
        public float2 MoveInput;           // X: strafe (A/D), Y: forward/back (W/S)

        // Mouse input
        public float2 LookInput;           // X: horizontal, Y: vertical

        // Action buttons
        public bool JumpPressed;
        public bool JumpHeld;
        public bool SprintPressed;
        public bool CrouchPressed;
        public bool PronePressed;

        // State toggles
        public bool IsSprintToggled;
        public bool IsCrouchToggled;

        // Frame tracking (for detecting button presses vs holds)
        public bool JumpPressedThisFrame;
    }
}
