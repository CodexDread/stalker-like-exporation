using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Component for world item UI display
    /// Handles distance-based name display above items
    /// Custom requirement: Show item name when within certain distance
    /// </summary>
    public struct WorldItemUIData : IComponentData
    {
        // Visibility state
        public bool IsVisible;                // Is UI currently visible?
        public float DistanceToPlayer;        // Current distance to player

        // Display settings
        public float FadeStartDistance;       // Distance to start fading (4m)
        public float FadeEndDistance;         // Distance at full visibility (2m)
        public float Alpha;                   // Current alpha (0-1)

        // Billboard settings
        public bool AlwaysFaceCamera;         // Should rotate to face camera?
        public float3 UIWorldPosition;        // World position of UI (item pos + offset)
    }
}
