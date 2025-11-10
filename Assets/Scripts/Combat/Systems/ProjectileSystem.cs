using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using ZoneSurvival.Weapons;

namespace ZoneSurvival.Combat
{
    /// <summary>
    /// Handles projectile/raycast hit detection for weapons
    /// Called when weapon fires - performs raycast and applies damage
    ///
    /// Two modes:
    /// 1. Hitscan (instant raycast) - for most guns
    /// 2. Projectile (physics-based) - for grenades, arrows (future)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WeaponFiringSystem))]
    public partial struct ProjectileSystem : ISystem
    {
        private Unity.Mathematics.Random random;

        public void OnCreate(ref SystemState state)
        {
            random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        }

        public void OnUpdate(ref SystemState state)
        {
            // Get physics world for raycasting
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var physicsWorld = physicsWorldSingleton.PhysicsWorld;

            // Query for weapons that fired this frame
            foreach (var (weaponState, weaponData, entity) in
                     SystemAPI.Query<RefRO<WeaponStateData>, RefRO<WeaponItemData>>()
                     .WithEntityAccess())
            {
                // Only process weapons that fired this frame
                if (!weaponState.ValueRO.IsFiring)
                    continue;

                // Perform hitscan
                PerformHitscan(ref state, entity, weaponState.ValueRO, weaponData.ValueRO, physicsWorld);
            }
        }

        /// <summary>
        /// Performs instant raycast from weapon muzzle
        /// </summary>
        private void PerformHitscan(ref SystemState state, Entity weaponEntity,
            WeaponStateData weaponState, WeaponItemData weaponData, PhysicsWorld physicsWorld)
        {
            // Get muzzle position and direction
            float3 rayStart = weaponState.MuzzlePosition;
            float3 rayDirection = math.forward(weaponState.MuzzleRotation);

            // Apply accuracy spread
            float spreadAngle = CalculateSpread(weaponState.CalculatedAccuracy);
            rayDirection = ApplySpread(rayDirection, spreadAngle, ref random);

            // Calculate ray end point
            float maxDistance = weaponState.CalculatedRange;
            float3 rayEnd = rayStart + rayDirection * maxDistance;

            // Setup raycast input
            var raycastInput = new RaycastInput
            {
                Start = rayStart,
                End = rayEnd,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,        // Everything
                    CollidesWith = ~0u,     // Everything
                    GroupIndex = 0
                }
            };

            // Perform raycast
            if (physicsWorld.CastRay(raycastInput, out RaycastHit hit))
            {
                // Hit something!
                Entity hitEntity = hit.Entity;

                // Check if hit entity has health
                if (state.EntityManager.HasComponent<HealthData>(hitEntity))
                {
                    ApplyDamage(ref state, hitEntity, weaponData, hit);
                }
                // Check if we hit a hitbox (for precise damage multipliers)
                else if (state.EntityManager.HasComponent<HitboxData>(hitEntity))
                {
                    var hitbox = state.EntityManager.GetComponentData<HitboxData>(hitEntity);
                    Entity parentEntity = hitbox.ParentEntity;

                    if (state.EntityManager.HasComponent<HealthData>(parentEntity))
                    {
                        ApplyDamageWithMultiplier(ref state, parentEntity, weaponData, hit, hitbox.DamageMultiplier);
                    }
                }

                // Spawn impact effect (handled by visual effects system)
                SpawnImpactEffect(ref state, hit.Position, hit.SurfaceNormal);
            }
        }

        /// <summary>
        /// Calculates weapon spread based on accuracy
        /// </summary>
        private float CalculateSpread(float accuracy)
        {
            // accuracy = 1.0 -> 0.5° spread
            // accuracy = 0.5 -> 5° spread
            // accuracy = 0.0 -> 10° spread
            float maxSpread = 10f; // degrees
            float minSpread = 0.5f; // degrees

            return math.lerp(maxSpread, minSpread, accuracy);
        }

        /// <summary>
        /// Applies random spread to ray direction
        /// </summary>
        private float3 ApplySpread(float3 direction, float spreadDegrees, ref Unity.Mathematics.Random rng)
        {
            // Convert to radians
            float spreadRadians = math.radians(spreadDegrees);

            // Random angle around circle
            float angle = rng.NextFloat() * 2f * math.PI;

            // Random distance from center (uniform distribution in cone)
            float distance = math.sqrt(rng.NextFloat()) * math.tan(spreadRadians);

            // Create perpendicular vectors
            float3 up = math.abs(direction.y) < 0.9f ? new float3(0, 1, 0) : new float3(1, 0, 0);
            float3 right = math.normalize(math.cross(up, direction));
            up = math.cross(direction, right);

            // Apply spread
            float3 spreadOffset = (right * math.cos(angle) + up * math.sin(angle)) * distance;
            return math.normalize(direction + spreadOffset);
        }

        /// <summary>
        /// Applies damage to target entity
        /// </summary>
        private void ApplyDamage(ref SystemState state, Entity target,
            WeaponItemData weaponData, RaycastHit hit)
        {
            ApplyDamageWithMultiplier(ref state, target, weaponData, hit, 1.0f);
        }

        /// <summary>
        /// Applies damage with hitbox multiplier (headshot, etc.)
        /// </summary>
        private void ApplyDamageWithMultiplier(ref SystemState state, Entity target,
            WeaponItemData weaponData, RaycastHit hit, float damageMultiplier)
        {
            // Add damage event component (processed by damage system)
            if (!state.EntityManager.HasComponent<DamageEvent>(target))
            {
                state.EntityManager.AddComponent<DamageEvent>(target);
            }

            // Calculate final damage
            float baseDamage = weaponData.BaseDamage;
            float finalDamage = baseDamage * damageMultiplier;

            // Set damage event data
            state.EntityManager.SetComponentData(target, new DamageEvent
            {
                Damage = finalDamage,
                ArmorPenetration = weaponData.ArmorPenetration,
                HitPosition = hit.Position,
                HitDirection = math.normalize(hit.Position - hit.Position), // TODO: Get from weapon
                Attacker = Entity.Null, // TODO: Track weapon owner
                Type = DamageType.Bullet
            });
        }

        /// <summary>
        /// Spawns visual impact effect
        /// TODO: Implement with visual effects system
        /// </summary>
        private void SpawnImpactEffect(ref SystemState state, float3 position, float3 normal)
        {
            // Placeholder - will be implemented with visual effects system
            // Create entity with ImpactEffectTag, position, normal
            // Visual effects system will spawn particle effect
        }
    }
}
