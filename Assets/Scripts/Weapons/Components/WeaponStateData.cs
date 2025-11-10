using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Tracks the current state and stats of an equipped weapon
    /// Updated each frame based on weapon parts and overall condition
    /// </summary>
    public struct WeaponStateData : IComponentData
    {
        // Equipment State
        public bool IsEquipped;                     // Currently held by player
        public bool IsHolstered;                    // In quick slot but not drawn
        public float DrawProgress;                  // 0.0-1.0 equip animation progress
        public float HolsterSpeed;                  // Time to draw/holster (seconds)

        // Fire Mode
        public FireMode CurrentFireMode;            // Current fire mode
        public FireMode AvailableFireModes;         // Bitmask of available modes

        // Ammo State
        public int CurrentMagazineAmmo;             // Rounds in current magazine
        public int MaxMagazineCapacity;             // Magazine size
        public int ReserveAmmo;                     // Ammo in inventory
        public bool IsChambered;                    // Round in chamber?

        // Firing State
        public bool IsFiring;                       // Currently firing
        public bool TriggerHeld;                    // Trigger is being held
        public float FireCooldown;                  // Time until can fire again
        public float TimeSinceLastShot;             // For fire rate calculation
        public int BurstShotsFired;                 // Shots fired in current burst

        // Reload State
        public bool IsReloading;                    // Currently reloading
        public float ReloadProgress;                // 0.0-1.0
        public float ReloadTime;                    // Total reload time (affected by parts)

        // Jamming State
        public bool IsJammed;                       // Weapon is jammed
        public float UnjamProgress;                 // 0.0-1.0 unjam animation
        public float UnjamTime;                     // Time to clear jam (1-3 seconds)
        public int ShotsSinceCleaning;              // Shots fired since last cleaning

        // Calculated Stats (updated from parts + base weapon + condition)
        public float CalculatedAccuracy;            // Final accuracy value
        public float CalculatedRecoil;              // Final recoil value
        public float CalculatedDamage;              // Final damage value
        public float CalculatedRange;               // Final effective range
        public float CalculatedJamChance;           // Current jam probability (0.0-1.0)
        public float CalculatedErgo;                // Ergonomics (affects ADS time)

        // Muzzle State (for effects)
        public float3 MuzzlePosition;               // World position of muzzle
        public quaternion MuzzleRotation;           // Direction weapon is pointing
    }

    /// <summary>
    /// Fire modes available (bitmask for multiple modes)
    /// </summary>
    [System.Flags]
    public enum FireMode : byte
    {
        None = 0,
        Safe = 1,           // Safety on, cannot fire
        Semi = 2,           // Semi-automatic (one shot per trigger pull)
        Burst = 4,          // Burst fire (3 rounds per trigger pull)
        Auto = 8,           // Full automatic
        BoltAction = 16     // Manual cycling required
    }

    /// <summary>
    /// Weapon equipped by player
    /// Tag component to identify the currently equipped weapon entity
    /// </summary>
    public struct EquippedWeaponTag : IComponentData
    {
        public Entity WeaponEntity;                 // Reference to weapon entity
        public int QuickSlotIndex;                  // Which quick slot (1-10)
    }
}
