using Unity.Entities;
using ZoneSurvival.Inventory;
using ZoneSurvival.Interaction;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Processes item pickup requests
    /// Transfers world items into player inventory
    /// Destroys world item entity on successful pickup
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InteractionExecutionSystem))]
    public partial struct ItemPickupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Get pickup requests (entities with PickupRequestTag)
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

            foreach (var (interactable, itemData, worldItem, entity) in
                     SystemAPI.Query<RefRO<InteractableTag>, RefRO<ItemData>,
                         RefRO<WorldItemTag>>().WithAll<PickupRequestTag>().WithEntityAccess())
            {
                // Find who requested the pickup
                Entity picker = interactable.ValueRO.InteractingEntity;

                if (picker == Entity.Null || !state.EntityManager.Exists(picker))
                {
                    // No valid picker - remove request
                    ecb.RemoveComponent<PickupRequestTag>(entity);
                    continue;
                }

                // Check if picker has inventory
                if (!state.EntityManager.HasComponent<InventoryData>(picker))
                {
                    ecb.RemoveComponent<PickupRequestTag>(entity);
                    continue;
                }

                // Try to add item to inventory
                bool success = InventoryManagementSystem.TryAddItem(ref state, picker, entity);

                if (success)
                {
                    // Item was added to inventory
                    // World item entity is destroyed by InventoryManagementSystem
                    // Just remove the pickup request tag
                    ecb.RemoveComponent<PickupRequestTag>(entity);

                    // Optional: Play pickup sound, show message, etc.
                    // (handled by audio/UI systems)
                }
                else
                {
                    // Failed to add (inventory full)
                    ecb.RemoveComponent<PickupRequestTag>(entity);

                    // Optional: Show "Inventory Full" message
                    // (handled by UI system)
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
