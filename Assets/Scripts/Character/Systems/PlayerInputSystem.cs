using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Captures player input from keyboard and mouse
    /// Converts Unity Input to ECS PlayerInputData component
    /// Based on GDD.md Controls section
    ///
    /// Controls:
    /// - WASD: Movement
    /// - Mouse: Look
    /// - Space: Jump
    /// - Shift: Sprint
    /// - Ctrl: Crouch
    /// - Z: Prone
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class PlayerInputSystem : SystemBase
    {
        private bool wasJumpPressed;

        protected override void OnUpdate()
        {
            // Capture input once per frame
            float2 moveInput = new float2(
                Input.GetAxisRaw("Horizontal"), // A/D
                Input.GetAxisRaw("Vertical")    // W/S
            );

            float2 lookInput = new float2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            );

            bool jumpPressed = Input.GetKey(KeyCode.Space);
            bool jumpPressedThisFrame = jumpPressed && !wasJumpPressed;
            wasJumpPressed = jumpPressed;

            bool sprintPressed = Input.GetKey(KeyCode.LeftShift);
            bool crouchPressed = Input.GetKey(KeyCode.LeftControl);
            bool pronePressed = Input.GetKey(KeyCode.Z);

            // Check for toggle mode (optional - can be added to settings later)
            // For now, using hold-to-sprint and hold-to-crouch
            bool isSprintToggled = false;
            bool isCrouchToggled = false;

            // Apply input to all player entities
            foreach (var input in SystemAPI.Query<RefRW<PlayerInputData>>())
            {
                input.ValueRW.MoveInput = moveInput;
                input.ValueRW.LookInput = lookInput;
                input.ValueRW.JumpPressed = jumpPressed;
                input.ValueRW.JumpHeld = jumpPressed;
                input.ValueRW.JumpPressedThisFrame = jumpPressedThisFrame;
                input.ValueRW.SprintPressed = sprintPressed;
                input.ValueRW.CrouchPressed = crouchPressed;
                input.ValueRW.PronePressed = pronePressed;
                input.ValueRW.IsSprintToggled = isSprintToggled;
                input.ValueRW.IsCrouchToggled = isCrouchToggled;
            }
        }
    }
}
