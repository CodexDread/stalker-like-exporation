using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Tag component for items that exist in the world
    /// Items with this component can be picked up
    /// </summary>
    public struct WorldItemTag : IComponentData
    {
        // World state
        public bool IsPickupable;             // Can be picked up right now?
        public float PickupRange;             // Distance at which item can be picked up (2m default)

        // Display
        public bool ShowNameUI;               // Should display name above item?
        public float NameDisplayDistance;     // Distance at which name appears (5m default)
        public float3 UIOffset;               // Offset from item position for UI (up by default)
    }
}
