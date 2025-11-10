using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Detects ground beneath character using raycasting
    /// Updates ground state, normal, and angle for movement system
    /// Handles coyote time for jump grace period
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    public partial struct GroundDetectionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            foreach (var (groundData, transform, physicsData, jumpData) in
                     SystemAPI.Query<RefRW<GroundDetectionData>, RefRO<LocalTransform>,
                         RefRO<CharacterPhysicsData>, RefRW<JumpData>>())
            {
                // Store previous ground state
                groundData.ValueRW.WasGroundedLastFrame = groundData.ValueRO.IsGrounded;
                groundData.ValueRW.JustLanded = false;

                // Calculate raycast start position (from bottom of capsule)
                float3 rayStart = transform.ValueRO.Position + groundData.ValueRO.GroundCheckOffset;
                float3 rayDirection = new float3(0, -1, 0);
                float rayDistance = groundData.ValueRO.GroundCheckDistance + 0.01f; // Slight extra distance

                // Perform sphere cast (better than raycast for character controller)
                var raycastInput = new RaycastInput
                {
                    Start = rayStart,
                    End = rayStart + rayDirection * rayDistance,
                    Filter = new CollisionFilter
                    {
                        BelongsTo = physicsData.ValueRO.CollisionLayer,
                        CollidesWith = physicsData.ValueRO.CollisionMask,
                        GroupIndex = 0
                    }
                };

                bool hitGround = physicsWorld.CastRay(raycastInput, out RaycastHit hit);

                if (hitGround)
                {
                    // We hit something
                    groundData.ValueRW.IsGrounded = true;
                    groundData.ValueRW.GroundNormal = hit.SurfaceNormal;
                    groundData.ValueRW.GroundDistance = hit.Fraction * rayDistance;
                    groundData.ValueRW.GroundEntity = hit.Entity;
                    groundData.ValueRW.TimeSinceGrounded = 0f;

                    // Calculate ground angle
                    float3 up = new float3(0, 1, 0);
                    groundData.ValueRW.GroundAngle = math.degrees(
                        math.acos(math.dot(hit.SurfaceNormal, up))
                    );

                    // Check if we just landed
                    if (!groundData.ValueRO.WasGroundedLastFrame)
                    {
                        groundData.ValueRW.JustLanded = true;
                    }

                    // Reset jump counters when grounded
                    if (groundData.ValueRW.JustLanded)
                    {
                        jumpData.ValueRW.IsJumping = false;
                        jumpData.ValueRW.JumpsRemaining = 1; // Reset jump count
                    }

                    // Reset coyote time when grounded
                    jumpData.ValueRW.CoyoteTimeCounter = jumpData.ValueRO.CoyoteTime;
                }
                else
                {
                    // Not grounded
                    groundData.ValueRW.IsGrounded = false;
                    groundData.ValueRW.GroundNormal = new float3(0, 1, 0);
                    groundData.ValueRW.GroundDistance = rayDistance;
                    groundData.ValueRW.GroundEntity = Entity.Null;
                    groundData.ValueRW.TimeSinceGrounded += deltaTime;

                    // Update coyote time (grace period after leaving ground)
                    if (groundData.ValueRO.WasGroundedLastFrame)
                    {
                        // Just left ground, start coyote time
                        jumpData.ValueRW.CoyoteTimeCounter = jumpData.ValueRO.CoyoteTime;
                    }
                    else
                    {
                        // Decrease coyote time
                        jumpData.ValueRW.CoyoteTimeCounter = math.max(0f,
                            jumpData.ValueRW.CoyoteTimeCounter - deltaTime);
                    }
                }

                // Update jump cooldown
                if (jumpData.ValueRW.JumpCooldownCounter > 0f)
                {
                    jumpData.ValueRW.JumpCooldownCounter -= deltaTime;
                }

                // Determine if can jump (grounded OR coyote time active AND cooldown done)
                jumpData.ValueRW.CanJump =
                    (groundData.ValueRO.IsGrounded || jumpData.ValueRO.CoyoteTimeCounter > 0f) &&
                    jumpData.ValueRO.JumpCooldownCounter <= 0f &&
                    jumpData.ValueRO.JumpsRemaining > 0;
            }
        }
    }
}
