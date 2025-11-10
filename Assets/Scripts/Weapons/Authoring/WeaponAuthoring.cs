using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using ZoneSurvival.Items;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Authoring component for creating weapons in Unity Editor
    /// Provides Inspector-based configuration for all weapon properties
    /// Bakes to ECS components: WeaponItemData, WeaponStateData, ItemData
    ///
    /// EASY WEAPON CREATION:
    /// 1. Create empty GameObject
    /// 2. Add this component
    /// 3. Configure values in Inspector
    /// 4. Enter Play mode - weapon entity created automatically
    /// </summary>
    public class WeaponAuthoring : MonoBehaviour
    {
        [Header("Weapon Identity")]
        [Tooltip("Unique weapon ID (for save/load)")]
        public int WeaponID = 1;

        [Tooltip("Display name of weapon")]
        public string WeaponName = "AK-74";

        [Tooltip("Type of weapon")]
        public WeaponType WeaponType = WeaponType.AssaultRifle;

        [Header("Item Properties")]
        [Tooltip("Item category (usually Weapon)")]
        public ItemCategory Category = ItemCategory.Weapon;

        [Tooltip("Item rarity")]
        public ItemRarity Rarity = ItemRarity.Common;

        [Tooltip("Weight in kg")]
        public float Weight = 3.5f;

        [Tooltip("Grid size (width)")]
        public int GridWidth = 4;

        [Tooltip("Grid size (height)")]
        public int GridHeight = 2;

        [Tooltip("Base value in Rubles")]
        public int BaseValue = 15000;

        [Tooltip("Starting condition (0.0 - 1.0)")]
        [Range(0f, 1f)]
        public float StartingCondition = 1.0f;

        [Header("Damage")]
        [Tooltip("Base damage per shot")]
        public float BaseDamage = 45f;

        [Tooltip("Armor penetration (0.0 - 1.0)")]
        [Range(0f, 1f)]
        public float ArmorPenetration = 0.6f;

        [Header("Fire Rate")]
        [Tooltip("Rounds per minute")]
        public float FireRate = 600f;

        [Tooltip("Available fire modes")]
        public FireMode AvailableFireModes = FireMode.Semi | FireMode.Auto;

        [Tooltip("Default fire mode on spawn")]
        public FireMode DefaultFireMode = FireMode.Semi;

        [Tooltip("Burst fire count (if burst mode available)")]
        public int BurstCount = 3;

        [Header("Magazine")]
        [Tooltip("Magazine capacity")]
        public int MagazineSize = 30;

        [Tooltip("Starting ammo in magazine")]
        public int StartingAmmo = 30;

        [Tooltip("Starting reserve ammo")]
        public int StartingReserveAmmo = 90;

        [Header("Accuracy & Recoil")]
        [Tooltip("Base accuracy (0.0 - 1.0, higher = more accurate)")]
        [Range(0f, 1f)]
        public float BaseAccuracy = 0.75f;

        [Tooltip("Recoil multiplier (higher = more recoil)")]
        public float RecoilMultiplier = 1.2f;

        [Tooltip("Time to aim down sights (seconds)")]
        public float AimDownSightTime = 0.35f;

        [Header("Range")]
        [Tooltip("Effective range in meters")]
        public float EffectiveRange = 300f;

        [Tooltip("Maximum range in meters")]
        public float MaxRange = 800f;

        [Header("Durability & Jamming")]
        [Tooltip("Condition lost per shot")]
        public float DegradationPerShot = 0.0001f;

        [Tooltip("Base jam chance when degraded (0.0 - 1.0)")]
        [Range(0f, 0.5f)]
        public float BaseJamChance = 0.01f;

        [Header("Part Slots (Optional - Advanced)")]
        [Tooltip("Define which parts this weapon can accept")]
        public WeaponPartSlotDefinitionAuthoring[] PartSlots = new WeaponPartSlotDefinitionAuthoring[]
        {
            new WeaponPartSlotDefinitionAuthoring { SlotType = WeaponPartType.Barrel, Required = true },
            new WeaponPartSlotDefinitionAuthoring { SlotType = WeaponPartType.Receiver, Required = true },
            new WeaponPartSlotDefinitionAuthoring { SlotType = WeaponPartType.Bolt, Required = true },
            new WeaponPartSlotDefinitionAuthoring { SlotType = WeaponPartType.FiringPin, Required = true },
            new WeaponPartSlotDefinitionAuthoring { SlotType = WeaponPartType.Trigger, Required = true },
            new WeaponPartSlotDefinitionAuthoring { SlotType = WeaponPartType.Magazine, Required = true },
        };

        /// <summary>
        /// Baker converts GameObject to ECS entity
        /// </summary>
        class Baker : Baker<WeaponAuthoring>
        {
            public override void Bake(WeaponAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Bake ItemData (base item properties)
                AddComponent(entity, new ItemData
                {
                    ItemID = authoring.WeaponID,
                    ItemName = new FixedString64Bytes(authoring.WeaponName),
                    Category = authoring.Category,
                    Rarity = authoring.Rarity,
                    Weight = authoring.Weight,
                    GridWidth = authoring.GridWidth,
                    GridHeight = authoring.GridHeight,
                    IsStackable = false, // Weapons don't stack
                    MaxStackSize = 1,
                    CurrentStackSize = 1,
                    BaseValue = authoring.BaseValue,
                    HasCondition = true,
                    Condition = authoring.StartingCondition
                });

                // Bake WeaponItemData (weapon-specific properties)
                AddComponent(entity, new WeaponItemData
                {
                    Type = authoring.WeaponType,
                    BaseDamage = authoring.BaseDamage,
                    ArmorPenetration = authoring.ArmorPenetration,
                    FireRate = authoring.FireRate,
                    IsAutomatic = (authoring.AvailableFireModes & FireMode.Auto) != 0,
                    IsSemiAuto = (authoring.AvailableFireModes & FireMode.Semi) != 0,
                    IsBurstFire = (authoring.AvailableFireModes & FireMode.Burst) != 0,
                    BurstCount = authoring.BurstCount,
                    MagazineSize = authoring.MagazineSize,
                    CurrentAmmo = authoring.StartingAmmo,
                    ReserveAmmo = authoring.StartingReserveAmmo,
                    BaseAccuracy = authoring.BaseAccuracy,
                    RecoilMultiplier = authoring.RecoilMultiplier,
                    AimDownSightTime = authoring.AimDownSightTime,
                    EffectiveRange = authoring.EffectiveRange,
                    MaxRange = authoring.MaxRange,
                    DegradationPerShot = authoring.DegradationPerShot,
                    JamChance = authoring.BaseJamChance
                });

                // Bake WeaponStateData (runtime state)
                AddComponent(entity, new WeaponStateData
                {
                    IsEquipped = false,
                    IsHolstered = false,
                    DrawProgress = 0f,
                    HolsterSpeed = authoring.AimDownSightTime,
                    CurrentFireMode = authoring.DefaultFireMode,
                    AvailableFireModes = authoring.AvailableFireModes,
                    CurrentMagazineAmmo = authoring.StartingAmmo,
                    MaxMagazineCapacity = authoring.MagazineSize,
                    ReserveAmmo = authoring.StartingReserveAmmo,
                    IsChambered = authoring.StartingAmmo > 0,
                    IsFiring = false,
                    TriggerHeld = false,
                    FireCooldown = 0f,
                    TimeSinceLastShot = 0f,
                    BurstShotsFired = 0,
                    IsReloading = false,
                    ReloadProgress = 0f,
                    ReloadTime = 2.5f,
                    IsJammed = false,
                    UnjamProgress = 0f,
                    UnjamTime = 2.0f,
                    ShotsSinceCleaning = 0,
                    // Initial calculated stats (will be updated by WeaponStatsCalculationSystem)
                    CalculatedAccuracy = authoring.BaseAccuracy,
                    CalculatedRecoil = authoring.RecoilMultiplier,
                    CalculatedDamage = authoring.BaseDamage,
                    CalculatedRange = authoring.EffectiveRange,
                    CalculatedJamChance = authoring.BaseJamChance,
                    CalculatedErgo = authoring.AimDownSightTime,
                    MuzzlePosition = Unity.Mathematics.float3.zero,
                    MuzzleRotation = Unity.Mathematics.quaternion.identity
                });

                // Bake WeaponPartElement buffer (initially empty, parts added later)
                var partsBuffer = AddBuffer<WeaponPartElement>(entity);

                // Bake WeaponPartSlotDefinition buffer (defines which parts can be attached)
                var slotsBuffer = AddBuffer<WeaponPartSlotDefinition>(entity);
                foreach (var slot in authoring.PartSlots)
                {
                    slotsBuffer.Add(new WeaponPartSlotDefinition
                    {
                        SlotType = slot.SlotType,
                        RequiredMount = slot.MountType,
                        IsRequired = slot.Required,
                        MaxCount = slot.MaxCount
                    });
                }
            }
        }
    }

    /// <summary>
    /// Helper class for part slot definition in Inspector
    /// </summary>
    [System.Serializable]
    public class WeaponPartSlotDefinitionAuthoring
    {
        public WeaponPartType SlotType = WeaponPartType.Barrel;
        public PartMountType MountType = PartMountType.Integrated;
        public bool Required = true;
        public int MaxCount = 1;
    }
}
