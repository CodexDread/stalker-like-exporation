using Unity.Entities;
using Unity.Mathematics;
using ZoneSurvival.Character;
using ZoneSurvival.Inventory;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Handles weapon reloading and unjamming
    /// - Reload from reserve ammo
    /// - Unjam weapon when jammed
    /// - Magazine swapping (future: different magazine types)
    ///
    /// Reads from PlayerInputData (unified input system)
    /// R key = reload, T key = unjam
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(WeaponFiringSystem))]
    public partial struct WeaponReloadSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Query for equipped weapons with player input
            foreach (var (weaponState, input) in SystemAPI.Query<RefRW<WeaponStateData>, RefRO<PlayerInputData>>())
            {
                // Only process equipped weapons
                if (!weaponState.ValueRO.IsEquipped || weaponState.ValueRO.IsHolstered)
                    continue;

                // === HANDLE UNJAMMING ===
                if (weaponState.ValueRO.IsJammed)
                {
                    // Start unjam on R key press (using interact for now, will add dedicated key)
                    if (input.ValueRO.InteractPressedThisFrame && weaponState.ValueRO.UnjamProgress == 0f)
                    {
                        // Start unjam
                        weaponState.ValueRW.UnjamTime = 2.0f; // 2 seconds to unjam
                    }

                    // Progress unjam
                    if (weaponState.ValueRO.UnjamProgress < 1.0f && weaponState.ValueRO.UnjamTime > 0f)
                    {
                        weaponState.ValueRW.UnjamProgress += deltaTime / weaponState.ValueRO.UnjamTime;

                        if (weaponState.ValueRW.UnjamProgress >= 1.0f)
                        {
                            // Unjam complete
                            weaponState.ValueRW.IsJammed = false;
                            weaponState.ValueRW.UnjamProgress = 0f;
                            // TODO: Play unjam sound
                        }
                    }

                    continue; // Cannot reload while jammed
                }

                // === HANDLE RELOADING ===
                if (weaponState.ValueRO.IsReloading)
                {
                    // Progress reload
                    weaponState.ValueRW.ReloadProgress += deltaTime / weaponState.ValueRO.ReloadTime;

                    if (weaponState.ValueRW.ReloadProgress >= 1.0f)
                    {
                        // Reload complete
                        CompleteReload(ref weaponState.ValueRW);
                    }
                }
                else
                {
                    // Check for reload input (R key - using SprintPressed for now as placeholder)
                    // TODO: Add dedicated reload key to PlayerInputData
                    bool reloadPressed = input.ValueRO.SprintPressed && !weaponState.ValueRO.IsFiring;

                    // Auto-reload when magazine empty and player presses fire with ammo available
                    bool autoReload = weaponState.ValueRO.CurrentMagazineAmmo == 0 &&
                                     input.ValueRO.InteractPressedThisFrame &&
                                     weaponState.ValueRO.ReserveAmmo > 0;

                    if (reloadPressed || autoReload)
                    {
                        StartReload(ref weaponState.ValueRW);
                    }
                }
            }
        }

        /// <summary>
        /// Starts reload animation/process
        /// </summary>
        private void StartReload(ref WeaponStateData weaponState)
        {
            // Check if reload is needed/possible
            if (weaponState.CurrentMagazineAmmo >= weaponState.MaxMagazineCapacity)
                return; // Magazine already full

            if (weaponState.ReserveAmmo <= 0)
                return; // No ammo to reload with

            if (weaponState.IsJammed)
                return; // Cannot reload while jammed

            // Start reload
            weaponState.IsReloading = true;
            weaponState.ReloadProgress = 0f;

            // Reload time affected by ergonomics
            float baseReloadTime = 2.5f; // Base 2.5 seconds
            weaponState.ReloadTime = baseReloadTime * weaponState.CalculatedErgo;

            // TODO: Play reload sound
        }

        /// <summary>
        /// Completes reload - transfers ammo from reserve to magazine
        /// </summary>
        private void CompleteReload(ref WeaponStateData weaponState)
        {
            weaponState.IsReloading = false;
            weaponState.ReloadProgress = 0f;

            // Calculate how much ammo to reload
            int ammoNeeded = weaponState.MaxMagazineCapacity - weaponState.CurrentMagazineAmmo;
            int ammoToLoad = math.min(ammoNeeded, weaponState.ReserveAmmo);

            // Transfer ammo
            weaponState.CurrentMagazineAmmo += ammoToLoad;
            weaponState.ReserveAmmo -= ammoToLoad;
            weaponState.IsChambered = true; // Chambered after reload

            // TODO: Play reload complete sound
        }
    }
}
