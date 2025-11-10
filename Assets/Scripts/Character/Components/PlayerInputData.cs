using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Unified player input data component
    /// Captures ALL player inputs for processing by systems
    /// Based on GDD.md Controls section
    ///
    /// Consolidated from multiple input systems for better organization
    /// and easier input rebinding in the future
    /// </summary>
    public struct PlayerInputData : IComponentData
    {
        // ===== MOVEMENT INPUT =====
        // Movement input (WASD)
        public float2 MoveInput;           // X: strafe (A/D), Y: forward/back (W/S)

        // Mouse input
        public float2 LookInput;           // X: horizontal, Y: vertical

        // ===== MOVEMENT ACTION BUTTONS =====
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

        // ===== INTERACTION INPUT =====
        // Interaction button (E key by default)
        public bool InteractPressed;           // Is button currently pressed?
        public bool InteractHeld;              // Is button held down?
        public bool InteractPressedThisFrame;  // Was button pressed this frame?

        // ===== INVENTORY INPUT =====
        // Inventory toggle (Tab key)
        public bool InventoryPressed;
        public bool InventoryToggled;          // Is inventory open?

        // Quick slots (1-0 keys for weapons/equipment)
        public int WeaponSlotPressed;          // Which weapon slot was pressed (1-10, 0 = none)

        // Consumable quick slots (F1-F4 keys)
        public int ConsumableSlotPressed;      // Which consumable slot was pressed (1-4, 0 = none)

        // Item drop (G key by default)
        public bool DropItemPressed;

        // ===== UI INPUT =====
        // PDA toggle (P key by default)
        public bool PDAPressed;
        public bool PDAToggled;
    }
}
