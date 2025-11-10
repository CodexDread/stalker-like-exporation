using Unity.Entities;
using Unity.Collections;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Component for weapon items
    /// Based on GDD.md weapon system (lines 470-525)
    /// </summary>
    public struct WeaponItemData : IComponentData
    {
        // Weapon type
        public WeaponType Type;

        // Damage
        public float BaseDamage;
        public float ArmorPenetration;        // 0.0-1.0 penetration value

        // Fire rate
        public float FireRate;                // Rounds per minute
        public bool IsAutomatic;              // Can hold trigger?
        public bool IsSemiAuto;               // One shot per trigger pull
        public bool IsBurstFire;              // Burst fire mode
        public int BurstCount;                // Shots per burst

        // Magazine
        public int MagazineSize;
        public int CurrentAmmo;
        public int ReserveAmmo;

        // Accuracy
        public float BaseAccuracy;            // 0.0-1.0
        public float RecoilMultiplier;        // Higher = more recoil
        public float AimDownSightTime;        // Time to ADS (seconds)

        // Range
        public float EffectiveRange;          // Meters
        public float MaxRange;                // Maximum bullet travel distance

        // Condition degradation
        public float DegradationPerShot;      // Condition lost per shot
        public float JamChance;               // Base jam chance when below 70% condition
    }

    /// <summary>
    /// Weapon types from GDD.md
    /// </summary>
    public enum WeaponType : byte
    {
        Pistol = 0,
        SMG = 1,
        AssaultRifle = 2,
        SniperRifle = 3,
        Shotgun = 4,
        Melee = 5
    }
}
