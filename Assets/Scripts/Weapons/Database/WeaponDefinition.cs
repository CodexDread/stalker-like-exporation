using UnityEngine;
using ZoneSurvival.Items;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// ScriptableObject database for weapon definitions
    /// Allows creating weapon presets as Unity assets
    ///
    /// EASIEST WAY TO ADD WEAPONS:
    /// 1. Right-click in Project window
    /// 2. Create -> Zone Survival -> Weapons -> Weapon Definition
    /// 3. Configure weapon in Inspector
    /// 4. Reference in spawning systems or use with WeaponDatabaseAuthoring
    ///
    /// Benefits:
    /// - Reusable weapon configurations
    /// - Easy to balance (edit asset, changes apply everywhere)
    /// - Can be loaded at runtime for modding support
    /// - Version control friendly (text-based assets)
    /// </summary>
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Zone Survival/Weapons/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [Header("Weapon Identity")]
        public int WeaponID = 1;
        public string WeaponName = "AK-74";
        public WeaponType WeaponType = WeaponType.AssaultRifle;

        [Header("Item Properties")]
        public ItemCategory Category = ItemCategory.Weapon;
        public ItemRarity Rarity = ItemRarity.Common;
        public float Weight = 3.5f;
        public int GridWidth = 4;
        public int GridHeight = 2;
        public int BaseValue = 15000;
        [Range(0f, 1f)]
        public float StartingCondition = 1.0f;

        [Header("Damage")]
        public float BaseDamage = 45f;
        [Range(0f, 1f)]
        public float ArmorPenetration = 0.6f;

        [Header("Fire Rate")]
        public float FireRate = 600f;
        public FireMode AvailableFireModes = FireMode.Semi | FireMode.Auto;
        public FireMode DefaultFireMode = FireMode.Semi;
        public int BurstCount = 3;

        [Header("Magazine")]
        public int MagazineSize = 30;
        public int StartingAmmo = 30;
        public int StartingReserveAmmo = 90;

        [Header("Accuracy & Recoil")]
        [Range(0f, 1f)]
        public float BaseAccuracy = 0.75f;
        public float RecoilMultiplier = 1.2f;
        public float AimDownSightTime = 0.35f;

        [Header("Range")]
        public float EffectiveRange = 300f;
        public float MaxRange = 800f;

        [Header("Durability & Jamming")]
        public float DegradationPerShot = 0.0001f;
        [Range(0f, 0.5f)]
        public float BaseJamChance = 0.01f;

        [Header("Default Parts")]
        [Tooltip("Parts that come pre-installed on this weapon")]
        public WeaponPartDefinition[] DefaultParts = new WeaponPartDefinition[0];

        [Header("Visual")]
        [Tooltip("3D model prefab for this weapon")]
        public GameObject ModelPrefab;

        [Tooltip("Icon for inventory UI")]
        public Sprite Icon;

        [Header("Audio")]
        public AudioClip FireSound;
        public AudioClip ReloadSound;
        public AudioClip DryFireSound;
        public AudioClip JamSound;

        /// <summary>
        /// Helper method to spawn this weapon as an ECS entity
        /// Can be called from spawning systems
        /// </summary>
        public void ApplyToAuthoring(WeaponAuthoring authoring)
        {
            authoring.WeaponID = WeaponID;
            authoring.WeaponName = WeaponName;
            authoring.WeaponType = WeaponType;
            authoring.Category = Category;
            authoring.Rarity = Rarity;
            authoring.Weight = Weight;
            authoring.GridWidth = GridWidth;
            authoring.GridHeight = GridHeight;
            authoring.BaseValue = BaseValue;
            authoring.StartingCondition = StartingCondition;
            authoring.BaseDamage = BaseDamage;
            authoring.ArmorPenetration = ArmorPenetration;
            authoring.FireRate = FireRate;
            authoring.AvailableFireModes = AvailableFireModes;
            authoring.DefaultFireMode = DefaultFireMode;
            authoring.BurstCount = BurstCount;
            authoring.MagazineSize = MagazineSize;
            authoring.StartingAmmo = StartingAmmo;
            authoring.StartingReserveAmmo = StartingReserveAmmo;
            authoring.BaseAccuracy = BaseAccuracy;
            authoring.RecoilMultiplier = RecoilMultiplier;
            authoring.AimDownSightTime = AimDownSightTime;
            authoring.EffectiveRange = EffectiveRange;
            authoring.MaxRange = MaxRange;
            authoring.DegradationPerShot = DegradationPerShot;
            authoring.BaseJamChance = BaseJamChance;
        }
    }
}
