using Unity.Entities;
using Unity.Mathematics;
using ZoneSurvival.Items;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Calculates weapon performance stats based on:
    /// 1. Base weapon stats (from WeaponItemData)
    /// 2. Overall weapon condition (from ItemData)
    /// 3. Individual part conditions and modifiers (from WeaponPartData)
    ///
    /// Two-Stage Durability System:
    /// - Overall weapon condition affects all stats globally
    /// - Each part's condition affects specific stats
    /// - Both contribute to jam chance
    ///
    /// Updates WeaponStateData.Calculated* fields
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(WeaponFiringSystem))]
    public partial struct WeaponStatsCalculationSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Query for equipped weapons that need stats recalculated
            foreach (var (weaponState, weaponData, itemData, partsBuffer) in
                     SystemAPI.Query<RefRW<WeaponStateData>, RefRO<WeaponItemData>, RefRO<ItemData>, DynamicBuffer<WeaponPartElement>>())
            {
                // Only calculate for equipped weapons (optimization)
                if (!weaponState.ValueRO.IsEquipped)
                    continue;

                // === BASE STATS FROM WEAPON ===
                float baseAccuracy = weaponData.ValueRO.BaseAccuracy;
                float baseRecoil = weaponData.ValueRO.RecoilMultiplier;
                float baseDamage = weaponData.ValueRO.BaseDamage;
                float baseRange = weaponData.ValueRO.EffectiveRange;
                float baseErgo = weaponData.ValueRO.AimDownSightTime;
                float baseJamChance = weaponData.ValueRO.JamChance;

                // === OVERALL WEAPON CONDITION MODIFIER ===
                float overallCondition = itemData.ValueRO.Condition; // 0.0 - 1.0

                // Condition affects all stats (worse condition = worse performance)
                float conditionModifier = math.lerp(0.5f, 1.0f, overallCondition); // 50% at 0%, 100% at 100%

                // Apply overall condition to base stats
                float accuracy = baseAccuracy * conditionModifier;
                float recoil = baseRecoil / conditionModifier; // Worse condition = MORE recoil
                float damage = baseDamage * conditionModifier;
                float range = baseRange * conditionModifier;
                float ergo = baseErgo / conditionModifier; // Worse condition = SLOWER ADS
                float jamChance = baseJamChance;

                // Jam chance increases as overall condition degrades
                if (overallCondition < 0.7f)
                {
                    jamChance += (0.7f - overallCondition) * 0.1f; // Up to +10% at 0%
                }

                // === PART-SPECIFIC MODIFIERS ===
                // Track critical parts for failure
                bool hasBarrel = false;
                bool hasFiringPin = false;
                bool hasBolt = false;

                float totalPartJamChance = 0f;

                // Iterate through all attached parts
                for (int i = 0; i < partsBuffer.Length; i++)
                {
                    Entity partEntity = partsBuffer[i].PartEntity;

                    // Check if part entity exists
                    if (!state.EntityManager.Exists(partEntity))
                        continue;

                    // Get part data
                    if (!state.EntityManager.HasComponent<WeaponPartData>(partEntity))
                        continue;

                    var part = state.EntityManager.GetComponentData<WeaponPartData>(partEntity);

                    // Track critical parts
                    switch (part.PartType)
                    {
                        case WeaponPartType.Barrel:
                            hasBarrel = true;
                            // Barrel condition heavily affects accuracy and range
                            accuracy += part.AccuracyModifier * part.Condition;
                            range += part.RangeModifier * part.Condition;
                            // Worn barrel increases jam chance
                            if (part.Condition < 0.5f)
                                totalPartJamChance += part.JamChanceWhenDegraded * (0.5f - part.Condition);
                            break;

                        case WeaponPartType.FiringPin:
                            hasFiringPin = true;
                            // Firing pin condition affects reliability (jam chance)
                            if (part.Condition < 0.5f)
                                totalPartJamChance += part.JamChanceWhenDegraded * (1.0f - part.Condition);
                            break;

                        case WeaponPartType.Bolt:
                            hasBolt = true;
                            // Bolt condition affects cycling (jam chance)
                            if (part.Condition < 0.5f)
                                totalPartJamChance += part.JamChanceWhenDegraded * (0.5f - part.Condition);
                            break;

                        case WeaponPartType.Receiver:
                            // Receiver affects overall reliability
                            if (part.Condition < 0.5f)
                                totalPartJamChance += part.JamChanceWhenDegraded * (0.3f - part.Condition);
                            break;

                        case WeaponPartType.Trigger:
                            // Trigger affects nothing major, mostly cosmetic/feel
                            break;

                        case WeaponPartType.Stock:
                            // Stock affects recoil and ergonomics
                            recoil += part.RecoilModifier;
                            ergo += part.ErgoModifier;
                            break;

                        case WeaponPartType.Grip:
                            // Grip affects recoil and ergonomics
                            recoil += part.RecoilModifier;
                            ergo += part.ErgoModifier;
                            break;

                        case WeaponPartType.Scope:
                            // Scope affects accuracy
                            accuracy += part.AccuracyModifier;
                            ergo += part.ErgoModifier; // Heavier scopes = slower ADS
                            break;

                        case WeaponPartType.Muzzle:
                            // Muzzle device (compensator, brake, suppressor)
                            recoil += part.RecoilModifier;
                            accuracy += part.AccuracyModifier;
                            break;

                        case WeaponPartType.Magazine:
                            // Magazine affects reliability
                            if (part.Condition < 0.5f)
                                totalPartJamChance += part.JamChanceWhenDegraded * (0.5f - part.Condition);
                            break;
                    }
                }

                // === CRITICAL PART MISSING CHECK ===
                // If critical parts are missing, weapon is non-functional
                if (!hasBarrel || !hasFiringPin || !hasBolt)
                {
                    // Weapon cannot fire - extremely high jam chance
                    jamChance = 1.0f;
                    accuracy = 0f;
                    damage = 0f;
                }
                else
                {
                    // Add part-specific jam chances
                    jamChance = math.clamp(jamChance + totalPartJamChance, 0f, 0.95f); // Max 95% jam chance
                }

                // === CLAMP FINAL VALUES ===
                accuracy = math.clamp(accuracy, 0f, 1.0f);
                recoil = math.max(recoil, 0.1f); // Minimum recoil
                damage = math.max(damage, 0f);
                range = math.max(range, 0f);
                ergo = math.max(ergo, 0.1f); // Minimum ADS time

                // === UPDATE WEAPON STATE ===
                weaponState.ValueRW.CalculatedAccuracy = accuracy;
                weaponState.ValueRW.CalculatedRecoil = recoil;
                weaponState.ValueRW.CalculatedDamage = damage;
                weaponState.ValueRW.CalculatedRange = range;
                weaponState.ValueRW.CalculatedErgo = ergo;
                weaponState.ValueRW.CalculatedJamChance = jamChance;
            }
        }
    }
}
