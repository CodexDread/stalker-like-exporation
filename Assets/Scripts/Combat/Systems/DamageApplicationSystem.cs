using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Combat
{
    /// <summary>
    /// Processes DamageEvent components and applies damage to HealthData
    /// Handles armor calculation, death detection
    /// Removes DamageEvent after processing
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ProjectileSystem))]
    public partial struct DamageApplicationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Update time since last damage for all entities with health
            foreach (var health in SystemAPI.Query<RefRW<HealthData>>())
            {
                health.ValueRW.TimeSinceLastDamage += deltaTime;
                health.ValueRW.DamageTakenThisFrame = 0f; // Reset each frame
            }

            // Process damage events
            foreach (var (damageEvent, health, entity) in
                     SystemAPI.Query<RefRO<DamageEvent>, RefRW<HealthData>>()
                     .WithEntityAccess())
            {
                // Skip if already dead or invulnerable
                if (health.ValueRO.IsDead || health.ValueRO.IsInvulnerable)
                {
                    state.EntityManager.RemoveComponent<DamageEvent>(entity);
                    continue;
                }

                // Calculate armor reduction
                float incomingDamage = damageEvent.ValueRO.Damage;
                float armorPenetration = damageEvent.ValueRO.ArmorPenetration;
                float armorValue = health.ValueRO.ArmorValue;

                // Armor effectiveness reduced by penetration
                float effectiveArmor = armorValue * (1.0f - armorPenetration);

                // Damage reduction formula: damage * (100 / (100 + armor))
                float damageReduction = 100f / (100f + effectiveArmor);
                float finalDamage = incomingDamage * damageReduction;

                // Degrade armor
                if (armorValue > 0f)
                {
                    float armorDamage = incomingDamage * 0.1f; // 10% of damage degrades armor
                    health.ValueRW.ArmorDurability -= armorDamage;

                    // Armor breaks if durability reaches 0
                    if (health.ValueRW.ArmorDurability <= 0f)
                    {
                        health.ValueRW.ArmorValue = 0f;
                        health.ValueRW.ArmorDurability = 0f;
                        // TODO: Spawn armor break effect
                    }
                }

                // Apply damage
                health.ValueRW.CurrentHealth -= finalDamage;
                health.ValueRW.DamageTakenThisFrame = finalDamage;
                health.ValueRW.TimeSinceLastDamage = 0f;

                // Check for death
                if (health.ValueRW.CurrentHealth <= 0f)
                {
                    health.ValueRW.CurrentHealth = 0f;
                    health.ValueRW.IsDead = true;

                    // Add death tag (processed by death system)
                    if (!state.EntityManager.HasComponent<DeathTag>(entity))
                    {
                        state.EntityManager.AddComponent<DeathTag>(entity);
                    }
                }

                // Remove damage event (processed)
                state.EntityManager.RemoveComponent<DamageEvent>(entity);
            }
        }
    }

    /// <summary>
    /// Tag component for entities that died this frame
    /// </summary>
    public struct DeathTag : IComponentData
    {
        public float TimeOfDeath;
        public Entity Killer;
    }
}
