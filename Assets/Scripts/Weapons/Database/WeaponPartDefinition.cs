using UnityEngine;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// ScriptableObject database for weapon part definitions
    /// Allows creating part presets as Unity assets
    ///
    /// EASIEST WAY TO ADD PARTS:
    /// 1. Right-click in Project window
    /// 2. Create -> Zone Survival -> Weapons -> Weapon Part Definition
    /// 3. Configure part in Inspector
    /// 4. Reference in WeaponDefinition or attach directly to weapons
    ///
    /// Benefits:
    /// - Reusable part configurations
    /// - Easy part library creation
    /// - Can be loaded at runtime for modding
    /// - Supports Tarkov-style gun building
    /// </summary>
    [CreateAssetMenu(fileName = "New Part", menuName = "Zone Survival/Weapons/Weapon Part Definition")]
    public class WeaponPartDefinition : ScriptableObject
    {
        [Header("Part Identity")]
        public int PartID = 1;
        public string PartName = "Standard Barrel";
        public WeaponPartType PartType = WeaponPartType.Barrel;
        public PartMountType MountType = PartMountType.Integrated;

        [Header("Condition")]
        [Range(0f, 1f)]
        public float StartingCondition = 1.0f;
        [Range(0f, 1f)]
        public float MaxCondition = 1.0f;
        public float DegradationRate = 0.00005f;

        [Header("Physical Properties")]
        public float Weight = 0.5f;
        public GameObject ModelPrefab;
        public bool IsVisible = true;

        [Header("Performance Modifiers")]
        [Range(-0.5f, 0.5f)]
        public float AccuracyModifier = 0f;
        [Range(-0.5f, 0.5f)]
        public float RecoilModifier = 0f;
        public float RangeModifier = 0f;
        [Range(-0.2f, 0.2f)]
        public float ErgoModifier = 0f;
        [Range(-0.2f, 0.2f)]
        public float DamageModifier = 0f;

        [Header("Reliability")]
        [Range(0f, 0.3f)]
        public float JamChanceWhenDegraded = 0.02f;

        [Header("Compatibility")]
        [Tooltip("Which weapons can use this part? (Empty = all weapons of compatible type)")]
        public WeaponDefinition[] CompatibleWeapons = new WeaponDefinition[0];

        [Header("Visual & Audio")]
        public Sprite Icon;
        public AudioClip AttachSound;
        public AudioClip DetachSound;

        [Header("Market")]
        public int BaseValue = 500;
        public bool IsTradeable = true;

        /// <summary>
        /// Helper method to apply this definition to a WeaponPartAuthoring component
        /// </summary>
        public void ApplyToAuthoring(WeaponPartAuthoring authoring)
        {
            authoring.PartID = PartID;
            authoring.PartName = PartName;
            authoring.PartType = PartType;
            authoring.MountType = MountType;
            authoring.StartingCondition = StartingCondition;
            authoring.MaxCondition = MaxCondition;
            authoring.DegradationRate = DegradationRate;
            authoring.Weight = Weight;
            authoring.IsVisible = IsVisible;
            authoring.AccuracyModifier = AccuracyModifier;
            authoring.RecoilModifier = RecoilModifier;
            authoring.RangeModifier = RangeModifier;
            authoring.ErgoModifier = ErgoModifier;
            authoring.DamageModifier = DamageModifier;
            authoring.JamChanceWhenDegraded = JamChanceWhenDegraded;
        }

        /// <summary>
        /// Check if this part is compatible with a given weapon
        /// </summary>
        public bool IsCompatibleWith(WeaponDefinition weapon)
        {
            // If no specific compatibility list, assume compatible with all
            if (CompatibleWeapons.Length == 0)
                return true;

            // Check if weapon is in compatibility list
            foreach (var compatibleWeapon in CompatibleWeapons)
            {
                if (compatibleWeapon == weapon)
                    return true;
            }

            return false;
        }
    }
}
