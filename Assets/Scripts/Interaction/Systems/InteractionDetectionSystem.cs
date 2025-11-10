using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using ZoneSurvival.Character;

namespace ZoneSurvival.Interaction
{
    /// <summary>
    /// Detects interactable objects in front of player using raycasting
    /// Updates InteractorData with current target
    /// Runs every frame to provide responsive interaction feedback
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct InteractionDetectionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            // Process all interactors (typically just the player)
            foreach (var (interactor, transform, camera) in
                     SystemAPI.Query<RefRW<InteractorData>, RefRO<LocalTransform>,
                         RefRO<FirstPersonCameraData>>())
            {
                // Calculate ray direction from camera
                float yawRadians = math.radians(camera.ValueRO.Yaw);
                float pitchRadians = math.radians(camera.ValueRO.Pitch);

                // Forward direction considering both yaw and pitch
                float3 forward = new float3(
                    math.sin(yawRadians) * math.cos(pitchRadians),
                    math.sin(pitchRadians),
                    math.cos(yawRadians) * math.cos(pitchRadians)
                );
                forward = math.normalize(forward);

                // Raycast start position (camera position)
                float3 rayStart = transform.ValueRO.Position + camera.ValueRO.CameraOffset;
                float3 rayEnd = rayStart + forward * interactor.ValueRO.RaycastDistance;

                // Perform raycast
                var raycastInput = new RaycastInput
                {
                    Start = rayStart,
                    End = rayEnd,
                    Filter = CollisionFilter.Default
                };

                Entity previousTarget = interactor.ValueRO.CurrentTarget;
                interactor.ValueRW.CurrentTarget = Entity.Null;

                if (physicsWorld.CastRay(raycastInput, out RaycastHit hit))
                {
                    Entity hitEntity = hit.Entity;

                    // Check if hit entity is interactable
                    if (SystemAPI.HasComponent<InteractableTag>(hitEntity))
                    {
                        var interactable = SystemAPI.GetComponent<InteractableTag>(hitEntity);

                        // Check if within interaction range
                        float distance = math.distance(transform.ValueRO.Position,
                            SystemAPI.GetComponent<LocalTransform>(hitEntity).Position);

                        if (distance <= interactable.InteractionRange && interactable.IsEnabled)
                        {
                            interactor.ValueRW.CurrentTarget = hitEntity;
                        }
                    }
                }

                // If target changed, could trigger highlight effect here
                // (handled by UI system or visual feedback system)
            }
        }
    }
}
