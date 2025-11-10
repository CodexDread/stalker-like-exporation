using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Authoring component for creating weapon parts in Unity Editor
    /// Makes it easy to add parts to the game (barrels, stocks, scopes, etc.)
    ///
    /// EASY PART CREATION:
    /// 1. Create empty GameObject
    /// 2. Add this component
    /// 3. Configure part type and modifiers in Inspector
    /// 4. Save as Prefab for reuse
    /// 5. Attach to weapons via WeaponPartElement buffer
    /// </summary>
    public class WeaponPartAuthoring : MonoBehaviour
    {
        [Header("Part Identity")]
        [Tooltip("Unique part ID")]
        public int PartID = 1;

        [Tooltip("Display name")]
        public string PartName = "Standard Barrel";

        [Tooltip("What type of part is this?")]
        public WeaponPartType PartType = WeaponPartType.Barrel;

        [Tooltip("How does this part mount?")]
        public PartMountType MountType = PartMountType.Integrated;

        [Header("Condition")]
        [Tooltip("Starting condition (0.0 - 1.0)")]
        [Range(0f, 1f)]
        public float StartingCondition = 1.0f;

        [Tooltip("Maximum achievable condition (repairs may lower this)")]
        [Range(0f, 1f)]
        public float MaxCondition = 1.0f;

        [Tooltip("Condition lost per shot (for parts that degrade)")]
        public float DegradationRate = 0.00005f;

        [Header("Physical Properties")]
        [Tooltip("Part weight in kg")]
        public float Weight = 0.5f;

        [Tooltip("3D model prefab ID (for visual attachment)")]
        public int PrefabID = 0;

        [Tooltip("Is this part visible on the weapon?")]
        public bool IsVisible = true;

        [Header("Performance Modifiers")]
        [Tooltip("Accuracy modifier (negative = worse, positive = better)")]
        [Range(-0.5f, 0.5f)]
        public float AccuracyModifier = 0f;

        [Tooltip("Recoil modifier (negative = less recoil, positive = more recoil)")]
        [Range(-0.5f, 0.5f)]
        public float RecoilModifier = 0f;

        [Tooltip("Range modifier in meters")]
        public float RangeModifier = 0f;

        [Tooltip("Ergonomics modifier (negative = faster ADS, positive = slower ADS)")]
        [Range(-0.2f, 0.2f)]
        public float ErgoModifier = 0f;

        [Tooltip("Damage modifier (percentage)")]
        [Range(-0.2f, 0.2f)]
        public float DamageModifier = 0f;

        [Header("Reliability")]
        [Tooltip("Jam chance when this part is degraded (<50% condition)")]
        [Range(0f, 0.3f)]
        public float JamChanceWhenDegraded = 0.02f;

        [Header("Preset Configurations")]
        [Tooltip("Quick preset selection (overrides manual settings)")]
        public PartPreset Preset = PartPreset.Custom;

        /// <summary>
        /// Baker converts GameObject to ECS entity
        /// </summary>
        class Baker : Baker<WeaponPartAuthoring>
        {
            public override void Bake(WeaponPartAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Apply preset if selected
                var config = authoring.Preset == PartPreset.Custom
                    ? GetCustomConfig(authoring)
                    : GetPresetConfig(authoring.Preset, authoring.PartType);

                // Override with custom values if preset is Custom
                if (authoring.Preset == PartPreset.Custom)
                {
                    config.AccuracyModifier = authoring.AccuracyModifier;
                    config.RecoilModifier = authoring.RecoilModifier;
                    config.RangeModifier = authoring.RangeModifier;
                    config.ErgoModifier = authoring.ErgoModifier;
                    config.DamageModifier = authoring.DamageModifier;
                }

                // Bake WeaponPartData
                AddComponent(entity, new WeaponPartData
                {
                    PartID = authoring.PartID,
                    PartName = new FixedString64Bytes(authoring.PartName),
                    PartType = authoring.PartType,
                    MountType = authoring.MountType,
                    Condition = authoring.StartingCondition,
                    DegradationRate = authoring.DegradationRate,
                    MaxCondition = authoring.MaxCondition,
                    Weight = authoring.Weight,
                    AccuracyModifier = config.AccuracyModifier,
                    RecoilModifier = config.RecoilModifier,
                    RangeModifier = config.RangeModifier,
                    ErgoModifier = config.ErgoModifier,
                    DamageModifier = config.DamageModifier,
                    JamChanceWhenDegraded = authoring.JamChanceWhenDegraded,
                    PrefabID = authoring.PrefabID,
                    IsVisible = authoring.IsVisible
                });
            }
        }

        private static PartConfig GetCustomConfig(WeaponPartAuthoring authoring)
        {
            return new PartConfig
            {
                AccuracyModifier = authoring.AccuracyModifier,
                RecoilModifier = authoring.RecoilModifier,
                RangeModifier = authoring.RangeModifier,
                ErgoModifier = authoring.ErgoModifier,
                DamageModifier = authoring.DamageModifier
            };
        }

        private static PartConfig GetPresetConfig(PartPreset preset, WeaponPartType partType)
        {
            // Preset configurations for common part types
            switch (preset)
            {
                case PartPreset.LowQuality:
                    return new PartConfig
                    {
                        AccuracyModifier = -0.1f,
                        RecoilModifier = 0.1f,
                        RangeModifier = -20f,
                        ErgoModifier = 0.05f,
                        DamageModifier = -0.05f
                    };

                case PartPreset.Standard:
                    return new PartConfig(); // All zeros

                case PartPreset.HighQuality:
                    return new PartConfig
                    {
                        AccuracyModifier = 0.1f,
                        RecoilModifier = -0.05f,
                        RangeModifier = 30f,
                        ErgoModifier = -0.02f,
                        DamageModifier = 0.05f
                    };

                case PartPreset.Tactical:
                    // Tactical parts prioritize ergonomics
                    return new PartConfig
                    {
                        AccuracyModifier = 0.05f,
                        RecoilModifier = -0.1f,
                        RangeModifier = 10f,
                        ErgoModifier = -0.1f,
                        DamageModifier = 0f
                    };

                case PartPreset.Sniper:
                    // Sniper parts prioritize accuracy and range
                    return new PartConfig
                    {
                        AccuracyModifier = 0.2f,
                        RecoilModifier = 0.05f,
                        RangeModifier = 100f,
                        ErgoModifier = 0.1f,
                        DamageModifier = 0.1f
                    };

                default:
                    return new PartConfig();
            }
        }
    }

    /// <summary>
    /// Preset configurations for common part quality levels
    /// </summary>
    public enum PartPreset
    {
        Custom,         // Manual configuration
        LowQuality,     // Cheap/worn parts (negative modifiers)
        Standard,       // Default parts (neutral)
        HighQuality,    // Premium parts (positive modifiers)
        Tactical,       // Optimized for CQB (low recoil, fast ADS)
        Sniper          // Optimized for long range (high accuracy/range)
    }

    /// <summary>
    /// Helper struct for part configuration
    /// </summary>
    public struct PartConfig
    {
        public float AccuracyModifier;
        public float RecoilModifier;
        public float RangeModifier;
        public float ErgoModifier;
        public float DamageModifier;
    }
}
