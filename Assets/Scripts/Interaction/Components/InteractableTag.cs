using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Interaction
{
    /// <summary>
    /// Tag component for objects that can be interacted with
    /// Used for doors, containers, NPCs, terminals, etc.
    /// </summary>
    public struct InteractableTag : IComponentData
    {
        // Interaction properties
        public InteractionType Type;
        public float InteractionRange;        // Distance at which interaction is possible (2m default)
        public float InteractionTime;         // Time required to complete interaction (0 = instant)

        // State
        public bool IsEnabled;                // Can be interacted with now?
        public bool RequiresInput;            // Does it need key press or automatic?
        public bool IsBeingInteracted;        // Currently being interacted with?
        public float InteractionProgress;     // 0.0 to 1.0 for timed interactions

        // Target
        public Entity InteractingEntity;      // Entity currently interacting (Entity.Null if none)
    }

    /// <summary>
    /// Types of interactions
    /// </summary>
    public enum InteractionType : byte
    {
        None = 0,
        Pickup = 1,          // Pick up item
        Open = 2,            // Open door/container
        Use = 3,             // Use device/terminal
        Talk = 4,            // Dialogue with NPC
        Loot = 5,            // Search container/body
        Repair = 6,          // Repair object
        Craft = 7,           // Use crafting station
        Trade = 8            // Trade with merchant
    }
}
