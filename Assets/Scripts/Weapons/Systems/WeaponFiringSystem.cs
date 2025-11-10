using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using ZoneSurvival.Character;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Handles weapon firing logic including:
    /// - Fire mode handling (semi, burst, auto)
    /// - Fire rate limiting
    /// - Ammo consumption
    /// - Jam chance calculation and execution
    /// - Trigger input processing
    ///
    /// Reads from PlayerInputData (unified input system)
    /// Updates weapon state and fires projectiles
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(WeaponStatsCalculationSystem))]
    public partial struct WeaponFiringSystem : ISystem
    {
        private Unity.Mathematics.Random random;

        public void OnCreate(ref SystemState state)
        {
            // Initialize random with seed
            random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        }

        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Query for equipped weapons with player input
            foreach (var (weaponState, input) in SystemAPI.Query<RefRW<WeaponStateData>, RefRO<PlayerInputData>>())
            {
                // Only process equipped weapons
                if (!weaponState.ValueRO.IsEquipped || weaponState.ValueRO.IsHolstered)
                    continue;

                // Update cooldowns
                if (weaponState.ValueRW.FireCooldown > 0f)
                {
                    weaponState.ValueRW.FireCooldown -= deltaTime;
                }

                weaponState.ValueRW.TimeSinceLastShot += deltaTime;

                // Cannot fire if reloading or jammed
                if (weaponState.ValueRO.IsReloading || weaponState.ValueRO.IsJammed)
                {
                    weaponState.ValueRW.IsFiring = false;
                    continue;
                }

                // Get fire input from unified input system
                // For now we'll use InteractPressed as fire button (will be changed to dedicated fire button later)
                bool firePressedThisFrame = input.ValueRO.InteractPressedThisFrame;
                bool fireHeld = input.ValueRO.InteractHeld;

                weaponState.ValueRW.TriggerHeld = fireHeld;

                // Check if player is trying to fire
                bool wantsToFire = false;
                FireMode currentMode = weaponState.ValueRO.CurrentFireMode;

                switch (currentMode)
                {
                    case FireMode.Safe:
                        // Safety on, cannot fire
                        wantsToFire = false;
                        break;

                    case FireMode.Semi:
                        // One shot per trigger pull
                        wantsToFire = firePressedThisFrame;
                        break;

                    case FireMode.Burst:
                        // Burst mode - fire burst on trigger pull
                        if (firePressedThisFrame)
                        {
                            weaponState.ValueRW.BurstShotsFired = 0;
                            wantsToFire = true;
                        }
                        else if (weaponState.ValueRO.BurstShotsFired > 0 && weaponState.ValueRO.BurstShotsFired < 3)
                        {
                            wantsToFire = true; // Continue burst
                        }
                        break;

                    case FireMode.Auto:
                        // Full auto - fire while trigger held
                        wantsToFire = fireHeld;
                        break;

                    case FireMode.BoltAction:
                        // Manual cycling - one shot per trigger pull, must release before next shot
                        wantsToFire = firePressedThisFrame;
                        break;
                }

                // Attempt to fire
                if (wantsToFire && weaponState.ValueRO.FireCooldown <= 0f)
                {
                    bool fired = AttemptFire(ref weaponState.ValueRW, ref random);

                    if (fired)
                    {
                        // Calculate fire rate cooldown
                        float roundsPerMinute = 600f; // Default, should come from weapon data
                        float secondsPerRound = 60f / roundsPerMinute;
                        weaponState.ValueRW.FireCooldown = secondsPerRound;
                        weaponState.ValueRW.TimeSinceLastShot = 0f;
                        weaponState.ValueRW.ShotsSinceCleaning++;

                        // Increment burst counter
                        if (currentMode == FireMode.Burst)
                        {
                            weaponState.ValueRW.BurstShotsFired++;
                        }

                        weaponState.ValueRW.IsFiring = true;

                        // TODO: Spawn projectile/raycast for hit detection
                        // TODO: Apply recoil
                        // TODO: Play fire sound/effect
                        // TODO: Degrade weapon parts condition
                    }
                    else
                    {
                        weaponState.ValueRW.IsFiring = false;
                    }
                }
                else
                {
                    weaponState.ValueRW.IsFiring = false;
                }
            }
        }

        /// <summary>
        /// Attempts to fire the weapon
        /// Returns true if shot was fired, false if jammed/no ammo
        /// </summary>
        private bool AttemptFire(ref WeaponStateData weaponState, ref Unity.Mathematics.Random rng)
        {
            // Check ammo
            if (weaponState.CurrentMagazineAmmo <= 0)
            {
                // Dry fire - click but no shot
                // TODO: Play dry fire sound
                return false;
            }

            // Check for jam
            if (CheckForJam(ref weaponState, ref rng))
            {
                weaponState.IsJammed = true;
                weaponState.UnjamProgress = 0f;
                // TODO: Play jam sound
                return false;
            }

            // Fire the weapon
            weaponState.CurrentMagazineAmmo--;

            // If this was last round, no longer chambered
            if (weaponState.CurrentMagazineAmmo == 0)
            {
                weaponState.IsChambered = false;
            }

            return true;
        }

        /// <summary>
        /// Checks if weapon jams based on condition and parts
        /// Returns true if weapon jammed
        /// </summary>
        private bool CheckForJam(ref WeaponStateData weaponState, ref Unity.Mathematics.Random rng)
        {
            float jamChance = weaponState.CalculatedJamChance;

            // Increased jam chance after many shots without cleaning
            if (weaponState.ShotsSinceCleaning > 500)
            {
                jamChance += 0.01f; // +1% per 500 rounds
            }

            // Roll for jam
            float roll = rng.NextFloat();
            return roll < jamChance;
        }
    }
}
