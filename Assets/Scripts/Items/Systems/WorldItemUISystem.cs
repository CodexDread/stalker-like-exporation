using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ZoneSurvival.Character;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Updates world item UI display based on distance to player
    /// Shows/hides item names, handles fading, billboard rotation
    /// Custom requirement: In-world rendered UI for item names
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CharacterMovementSystem))]
    public partial class WorldItemUISystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Find player position (assumes single player with FirstPersonCameraData)
            float3 playerPosition = float3.zero;
            bool hasPlayer = false;

            foreach (var (transform, camera) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<FirstPersonCameraData>>())
            {
                playerPosition = transform.ValueRO.Position;
                hasPlayer = true;
                break; // Only need first player
            }

            if (!hasPlayer)
                return;

            // Update all world items with UI
            foreach (var (worldItem, itemData, uiData, transform) in
                     SystemAPI.Query<RefRO<WorldItemTag>, RefRO<ItemData>,
                         RefRW<WorldItemUIData>, RefRO<LocalTransform>>())
            {
                if (!worldItem.ValueRO.ShowNameUI)
                {
                    uiData.ValueRW.IsVisible = false;
                    continue;
                }

                // Calculate distance to player
                float distance = math.distance(playerPosition, transform.ValueRO.Position);
                uiData.ValueRW.DistanceToPlayer = distance;

                // Calculate UI world position (item position + offset)
                uiData.ValueRW.UIWorldPosition = transform.ValueRO.Position + worldItem.ValueRO.UIOffset;

                // Check if within display distance
                if (distance <= worldItem.ValueRO.NameDisplayDistance)
                {
                    uiData.ValueRW.IsVisible = true;

                    // Calculate alpha based on distance (fade in/out)
                    float fadeStart = uiData.ValueRO.FadeStartDistance;
                    float fadeEnd = uiData.ValueRO.FadeEndDistance;

                    if (distance >= fadeStart)
                    {
                        // Fading out
                        float fadeRange = worldItem.ValueRO.NameDisplayDistance - fadeStart;
                        float fadeAmount = (distance - fadeStart) / fadeRange;
                        uiData.ValueRW.Alpha = 1.0f - math.saturate(fadeAmount);
                    }
                    else if (distance <= fadeEnd)
                    {
                        // Fully visible
                        uiData.ValueRW.Alpha = 1.0f;
                    }
                    else
                    {
                        // Fading in
                        float fadeRange = fadeStart - fadeEnd;
                        float fadeAmount = (distance - fadeEnd) / fadeRange;
                        uiData.ValueRW.Alpha = 1.0f - math.saturate(fadeAmount);
                    }
                }
                else
                {
                    uiData.ValueRW.IsVisible = false;
                    uiData.ValueRW.Alpha = 0f;
                }
            }
        }
    }
}
