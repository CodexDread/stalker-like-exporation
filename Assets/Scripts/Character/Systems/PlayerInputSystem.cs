using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// UNIFIED player input system - captures ALL player inputs
    /// Converts Unity Input to ECS PlayerInputData component
    /// Based on GDD.md Controls section
    ///
    /// Complete Controls Reference:
    /// MOVEMENT:
    /// - WASD: Movement
    /// - Mouse: Look
    /// - Space: Jump
    /// - Shift: Sprint
    /// - Ctrl: Crouch
    /// - Z: Prone
    ///
    /// INTERACTION:
    /// - E: Interact
    ///
    /// INVENTORY:
    /// - Tab: Toggle Inventory
    /// - 1-0: Weapon Quick Slots
    /// - F1-F4: Consumable Quick Slots
    /// - G: Drop Item
    ///
    /// UI:
    /// - P: Toggle PDA
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class PlayerInputSystem : SystemBase
    {
        // Previous frame state tracking for "pressed this frame" detection
        private bool wasJumpPressed;
        private bool wasInteractPressed;
        private bool wasInventoryPressed;
        private bool wasPDAPressed;

        protected override void OnUpdate()
        {
            // ===== MOVEMENT INPUT =====
            float2 moveInput = new float2(
                Input.GetAxisRaw("Horizontal"), // A/D
                Input.GetAxisRaw("Vertical")    // W/S
            );

            float2 lookInput = new float2(
                Input.GetAxisRaw("Mouse X"),
                Input.GetAxisRaw("Mouse Y")
            );

            // ===== MOVEMENT ACTIONS =====
            bool jumpPressed = Input.GetKey(KeyCode.Space);
            bool jumpPressedThisFrame = jumpPressed && !wasJumpPressed;
            wasJumpPressed = jumpPressed;

            bool sprintPressed = Input.GetKey(KeyCode.LeftShift);
            bool crouchPressed = Input.GetKey(KeyCode.LeftControl);
            bool pronePressed = Input.GetKey(KeyCode.Z);

            // Toggle modes (can be configured later)
            bool isSprintToggled = false;
            bool isCrouchToggled = false;

            // ===== INTERACTION INPUT =====
            bool interactPressed = Input.GetKey(KeyCode.E);
            bool interactPressedThisFrame = interactPressed && !wasInteractPressed;
            wasInteractPressed = interactPressed;

            // ===== INVENTORY INPUT =====
            bool inventoryPressed = Input.GetKeyDown(KeyCode.Tab);
            bool inventoryToggled = Input.GetKey(KeyCode.Tab); // Can track toggle state

            // Weapon quick slots (1-0 keys)
            int weaponSlotPressed = 0;
            if (Input.GetKeyDown(KeyCode.Alpha1)) weaponSlotPressed = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) weaponSlotPressed = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) weaponSlotPressed = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) weaponSlotPressed = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) weaponSlotPressed = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) weaponSlotPressed = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha7)) weaponSlotPressed = 7;
            else if (Input.GetKeyDown(KeyCode.Alpha8)) weaponSlotPressed = 8;
            else if (Input.GetKeyDown(KeyCode.Alpha9)) weaponSlotPressed = 9;
            else if (Input.GetKeyDown(KeyCode.Alpha0)) weaponSlotPressed = 10;

            // Consumable quick slots (F1-F4 keys)
            int consumableSlotPressed = 0;
            if (Input.GetKeyDown(KeyCode.F1)) consumableSlotPressed = 1;
            else if (Input.GetKeyDown(KeyCode.F2)) consumableSlotPressed = 2;
            else if (Input.GetKeyDown(KeyCode.F3)) consumableSlotPressed = 3;
            else if (Input.GetKeyDown(KeyCode.F4)) consumableSlotPressed = 4;

            bool dropItemPressed = Input.GetKeyDown(KeyCode.G);

            // ===== UI INPUT =====
            bool pdaPressed = Input.GetKeyDown(KeyCode.P);
            bool pdaToggled = Input.GetKey(KeyCode.P);

            // ===== APPLY TO ALL PLAYER ENTITIES =====
            foreach (var input in SystemAPI.Query<RefRW<PlayerInputData>>())
            {
                // Movement
                input.ValueRW.MoveInput = moveInput;
                input.ValueRW.LookInput = lookInput;

                // Movement actions
                input.ValueRW.JumpPressed = jumpPressed;
                input.ValueRW.JumpHeld = jumpPressed;
                input.ValueRW.JumpPressedThisFrame = jumpPressedThisFrame;
                input.ValueRW.SprintPressed = sprintPressed;
                input.ValueRW.CrouchPressed = crouchPressed;
                input.ValueRW.PronePressed = pronePressed;
                input.ValueRW.IsSprintToggled = isSprintToggled;
                input.ValueRW.IsCrouchToggled = isCrouchToggled;

                // Interaction
                input.ValueRW.InteractPressed = interactPressed;
                input.ValueRW.InteractHeld = interactPressed;
                input.ValueRW.InteractPressedThisFrame = interactPressedThisFrame;

                // Inventory
                input.ValueRW.InventoryPressed = inventoryPressed;
                input.ValueRW.InventoryToggled = inventoryToggled;
                input.ValueRW.WeaponSlotPressed = weaponSlotPressed;
                input.ValueRW.ConsumableSlotPressed = consumableSlotPressed;
                input.ValueRW.DropItemPressed = dropItemPressed;

                // UI
                input.ValueRW.PDAPressed = pdaPressed;
                input.ValueRW.PDAToggled = pdaToggled;
            }
        }
    }
}
