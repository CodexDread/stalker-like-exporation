using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Handles weapon visual effects:
    /// - Muzzle flash when firing
    /// - Shell ejection
    /// - Bullet tracers
    /// - Gun smoke
    ///
    /// Processes WeaponFireEffectRequest tags and spawns effects
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WeaponFiringSystem))]
    public partial struct WeaponVisualEffectsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Update active effect timers
            foreach (var effectsData in SystemAPI.Query<RefRW<WeaponVisualEffectsData>>())
            {
                // Update muzzle flash timer
                if (effectsData.ValueRO.MuzzleFlashTimer > 0f)
                {
                    effectsData.ValueRW.MuzzleFlashTimer -= deltaTime;
                }

                // Update shell ejection timer
                if (effectsData.ValueRO.ShellEjectionTimer > 0f)
                {
                    effectsData.ValueRW.ShellEjectionTimer -= deltaTime;

                    // Time to eject shell
                    if (effectsData.ValueRW.ShellEjectionTimer <= 0f)
                    {
                        // Eject shell (handled below)
                        effectsData.ValueRW.ShellEjectionTimer = 0f;
                    }
                }
            }

            // Process fire effect requests
            foreach (var (fireRequest, effectsData, weaponState, entity) in
                     SystemAPI.Query<RefRO<WeaponFireEffectRequest>, RefRW<WeaponVisualEffectsData>, RefRO<WeaponStateData>>()
                     .WithEntityAccess())
            {
                // Spawn muzzle flash
                if (effectsData.ValueRO.MuzzleFlashPrefabID != 0)
                {
                    SpawnMuzzleFlash(ref state, fireRequest.ValueRO, effectsData.ValueRO);
                    effectsData.ValueRW.MuzzleFlashTimer = effectsData.ValueRO.MuzzleFlashDuration;
                }

                // Schedule shell ejection
                if (effectsData.ValueRO.EjectsShells)
                {
                    effectsData.ValueRW.ShellEjectionTimer = effectsData.ValueRO.EjectionDelay;
                    SpawnShellCasing(ref state, fireRequest.ValueRO, effectsData.ValueRO);
                }

                // Spawn tracer
                if (effectsData.ValueRO.HasTracer && fireRequest.ValueRO.DidHit)
                {
                    SpawnTracer(ref state, fireRequest.ValueRO, effectsData.ValueRO);
                }

                // Spawn smoke
                if (effectsData.ValueRO.HasSmoke)
                {
                    SpawnSmoke(ref state, fireRequest.ValueRO, effectsData.ValueRO);
                }

                // Remove request (processed)
                state.EntityManager.RemoveComponent<WeaponFireEffectRequest>(entity);
            }
        }

        /// <summary>
        /// Spawns muzzle flash particle effect
        /// </summary>
        private void SpawnMuzzleFlash(ref SystemState state,
            WeaponFireEffectRequest request, WeaponVisualEffectsData effectsData)
        {
            // Create muzzle flash entity
            Entity flashEntity = state.EntityManager.CreateEntity();

            // Add transform
            state.EntityManager.AddComponentData(flashEntity, LocalTransform.FromPositionRotation(
                request.MuzzlePosition,
                request.MuzzleRotation
            ));

            // Add visual effect component (processed by rendering system)
            state.EntityManager.AddComponentData(flashEntity, new VisualEffectData
            {
                PrefabID = effectsData.MuzzleFlashPrefabID,
                Lifetime = effectsData.MuzzleFlashDuration,
                Scale = effectsData.MuzzleFlashScale,
                TimeRemaining = effectsData.MuzzleFlashDuration
            });

            // Tag as temporary effect (auto-destroyed after lifetime)
            state.EntityManager.AddComponent<TemporaryEffectTag>(flashEntity);
        }

        /// <summary>
        /// Spawns shell casing with physics
        /// </summary>
        private void SpawnShellCasing(ref SystemState state,
            WeaponFireEffectRequest request, WeaponVisualEffectsData effectsData)
        {
            // Calculate ejection position (offset from muzzle)
            float3 ejectionPos = request.MuzzlePosition +
                                math.rotate(request.MuzzleRotation, effectsData.EjectionOffset);

            // Calculate ejection velocity (local to world space)
            float3 ejectionVel = math.rotate(request.MuzzleRotation, effectsData.EjectionVelocity);

            // Create shell entity
            Entity shellEntity = state.EntityManager.CreateEntity();

            // Add transform
            state.EntityManager.AddComponentData(shellEntity, LocalTransform.FromPositionRotation(
                ejectionPos,
                request.MuzzleRotation
            ));

            // Add physics velocity (requires Unity.Physics)
            // TODO: Add PhysicsVelocity component with ejectionVel

            // Add visual
            state.EntityManager.AddComponentData(shellEntity, new VisualEffectData
            {
                PrefabID = effectsData.ShellPrefabID,
                Lifetime = 5.0f, // Shells last 5 seconds before despawn
                Scale = 1.0f,
                TimeRemaining = 5.0f
            });

            state.EntityManager.AddComponent<TemporaryEffectTag>(shellEntity);
        }

        /// <summary>
        /// Spawns bullet tracer line effect
        /// </summary>
        private void SpawnTracer(ref SystemState state,
            WeaponFireEffectRequest request, WeaponVisualEffectsData effectsData)
        {
            Entity tracerEntity = state.EntityManager.CreateEntity();

            // Add tracer component
            state.EntityManager.AddComponentData(tracerEntity, new TracerEffectData
            {
                StartPosition = request.MuzzlePosition,
                EndPosition = request.HitPosition,
                Speed = effectsData.TracerSpeed,
                CurrentPosition = request.MuzzlePosition,
                PrefabID = effectsData.TracerPrefabID,
                Lifetime = effectsData.TracerLifetime,
                TimeRemaining = effectsData.TracerLifetime
            });

            state.EntityManager.AddComponent<TemporaryEffectTag>(tracerEntity);
        }

        /// <summary>
        /// Spawns gun smoke effect
        /// </summary>
        private void SpawnSmoke(ref SystemState state,
            WeaponFireEffectRequest request, WeaponVisualEffectsData effectsData)
        {
            Entity smokeEntity = state.EntityManager.CreateEntity();

            state.EntityManager.AddComponentData(smokeEntity, LocalTransform.FromPositionRotation(
                request.MuzzlePosition,
                request.MuzzleRotation
            ));

            state.EntityManager.AddComponentData(smokeEntity, new VisualEffectData
            {
                PrefabID = effectsData.SmokePrefabID,
                Lifetime = effectsData.SmokeDuration,
                Scale = 1.0f,
                TimeRemaining = effectsData.SmokeDuration
            });

            state.EntityManager.AddComponent<TemporaryEffectTag>(smokeEntity);
        }
    }

    /// <summary>
    /// Generic visual effect data
    /// </summary>
    public struct VisualEffectData : IComponentData
    {
        public int PrefabID;
        public float Lifetime;
        public float Scale;
        public float TimeRemaining;
    }

    /// <summary>
    /// Tracer-specific effect data
    /// </summary>
    public struct TracerEffectData : IComponentData
    {
        public float3 StartPosition;
        public float3 EndPosition;
        public float3 CurrentPosition;
        public float Speed;
        public int PrefabID;
        public float Lifetime;
        public float TimeRemaining;
    }

    /// <summary>
    /// Tag for temporary effects that auto-destroy
    /// </summary>
    public struct TemporaryEffectTag : IComponentData { }

    /// <summary>
    /// Cleanup system - destroys temporary effects after lifetime expires
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WeaponVisualEffectsSystem))]
    public partial struct TemporaryEffectCleanupSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Update and destroy expired visual effects
            foreach (var (effectData, entity) in
                     SystemAPI.Query<RefRW<VisualEffectData>>()
                     .WithAll<TemporaryEffectTag>()
                     .WithEntityAccess())
            {
                effectData.ValueRW.TimeRemaining -= deltaTime;

                if (effectData.ValueRW.TimeRemaining <= 0f)
                {
                    state.EntityManager.DestroyEntity(entity);
                }
            }

            // Update and destroy expired tracers
            foreach (var (tracerData, entity) in
                     SystemAPI.Query<RefRW<TracerEffectData>>()
                     .WithAll<TemporaryEffectTag>()
                     .WithEntityAccess())
            {
                tracerData.ValueRW.TimeRemaining -= deltaTime;

                // Move tracer along path
                float progress = 1.0f - (tracerData.ValueRW.TimeRemaining / tracerData.ValueRW.Lifetime);
                tracerData.ValueRW.CurrentPosition = math.lerp(
                    tracerData.ValueRO.StartPosition,
                    tracerData.ValueRO.EndPosition,
                    progress
                );

                if (tracerData.ValueRW.TimeRemaining <= 0f)
                {
                    state.EntityManager.DestroyEntity(entity);
                }
            }
        }
    }
}
