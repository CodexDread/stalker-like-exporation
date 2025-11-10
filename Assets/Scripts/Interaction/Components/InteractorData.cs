using Unity.Entities;

namespace ZoneSurvival.Interaction
{
    /// <summary>
    /// Component for entities that can interact with objects
    /// Typically attached to player character
    /// </summary>
    public struct InteractorData : IComponentData
    {
        // Current interaction state
        public Entity CurrentTarget;          // Entity being looked at (Entity.Null if none)
        public Entity InteractingWith;        // Entity currently interacting with
        public float InteractionProgress;     // Progress of current interaction (0.0-1.0)

        // Configuration
        public float InteractionRange;        // Maximum interaction distance (2m default)
        public float RaycastDistance;         // Distance to check for interactables (3m default)

        // Input
        public bool InteractPressed;          // Was interact button pressed this frame?
        public bool InteractHeld;             // Is interact button held?

        // Cooldown
        public float InteractionCooldown;     // Time before next interaction (0.2s)
        public float CooldownTimer;           // Current cooldown remaining
    }
}
