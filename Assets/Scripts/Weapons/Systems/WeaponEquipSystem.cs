using Unity.Entities;
using Unity.Mathematics;
using ZoneSurvival.Character;
using ZoneSurvival.Inventory;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Handles weapon equipping and holstering
    /// - Equip weapon from quick slots (1-0 keys)
    /// - Holster current weapon
    /// - Draw animations and timing
    ///
    /// Reads from PlayerInputData (unified input system)
    /// Uses WeaponSlotPressed (1-10) to select weapon
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(WeaponFiringSystem))]
    public partial struct WeaponEquipSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            // Query for player with quick slots and input
            foreach (var (quickSlots, input, entity) in
                     SystemAPI.Query<RefRO<QuickSlotsData>, RefRO<PlayerInputData>>()
                     .WithEntityAccess())
            {
                int weaponSlotPressed = input.ValueRO.WeaponSlotPressed;

                // Check if player pressed a weapon slot key (1-0)
                if (weaponSlotPressed > 0)
                {
                    // Attempt to equip weapon from that slot
                    AttemptEquipFromSlot(ref state, entity, weaponSlotPressed);
                }

                // TODO: Handle holster key (H key)
                // For now, pressing same slot again holsters
            }

            // Update draw/holster progress for all weapons
            foreach (var weaponState in SystemAPI.Query<RefRW<WeaponStateData>>())
            {
                // Progress draw animation
                if (weaponState.ValueRO.IsEquipped && !weaponState.ValueRO.IsHolstered)
                {
                    if (weaponState.ValueRO.DrawProgress < 1.0f)
                    {
                        weaponState.ValueRW.DrawProgress += deltaTime / weaponState.ValueRO.HolsterSpeed;
                        weaponState.ValueRW.DrawProgress = math.min(weaponState.ValueRW.DrawProgress, 1.0f);
                    }
                }
                // Progress holster animation
                else if (weaponState.ValueRO.IsHolstered)
                {
                    if (weaponState.ValueRO.DrawProgress > 0.0f)
                    {
                        weaponState.ValueRW.DrawProgress -= deltaTime / weaponState.ValueRO.HolsterSpeed;
                        weaponState.ValueRW.DrawProgress = math.max(weaponState.ValueRW.DrawProgress, 0.0f);

                        // Fully holstered
                        if (weaponState.ValueRW.DrawProgress <= 0.0f)
                        {
                            weaponState.ValueRW.IsEquipped = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to equip weapon from specified quick slot
        /// </summary>
        private void AttemptEquipFromSlot(ref SystemState state, Entity playerEntity, int slotIndex)
        {
            // Get player's quick slots
            if (!state.EntityManager.HasComponent<QuickSlotsData>(playerEntity))
                return;

            var quickSlots = state.EntityManager.GetComponentData<QuickSlotsData>(playerEntity);

            // Validate slot index (1-10 -> 0-9 array index)
            int arrayIndex = slotIndex - 1;
            if (arrayIndex < 0 || arrayIndex >= 10)
                return;

            Entity weaponEntity = quickSlots.WeaponSlots[arrayIndex];

            // Check if slot has a weapon
            if (weaponEntity == Entity.Null || !state.EntityManager.Exists(weaponEntity))
            {
                // Slot is empty - holster current weapon if any
                HolsterAllWeapons(ref state, playerEntity);
                return;
            }

            // Check if this weapon is already equipped
            if (state.EntityManager.HasComponent<EquippedWeaponTag>(playerEntity))
            {
                var currentEquipped = state.EntityManager.GetComponentData<EquippedWeaponTag>(playerEntity);
                if (currentEquipped.WeaponEntity == weaponEntity)
                {
                    // Same weapon - toggle holster
                    if (state.EntityManager.HasComponent<WeaponStateData>(weaponEntity))
                    {
                        var weaponState = state.EntityManager.GetComponentData<WeaponStateData>(weaponEntity);
                        weaponState.IsHolstered = !weaponState.IsHolstered;
                        state.EntityManager.SetComponentData(weaponEntity, weaponState);
                    }
                    return;
                }
            }

            // Holster any currently equipped weapons
            HolsterAllWeapons(ref state, playerEntity);

            // Equip new weapon
            if (state.EntityManager.HasComponent<WeaponStateData>(weaponEntity))
            {
                var weaponState = state.EntityManager.GetComponentData<WeaponStateData>(weaponEntity);
                weaponState.IsEquipped = true;
                weaponState.IsHolstered = false;
                weaponState.DrawProgress = 0f;
                weaponState.HolsterSpeed = weaponState.CalculatedErgo; // Draw speed from ergonomics
                state.EntityManager.SetComponentData(weaponEntity, weaponState);

                // Tag player with equipped weapon
                if (state.EntityManager.HasComponent<EquippedWeaponTag>(playerEntity))
                {
                    var equipped = state.EntityManager.GetComponentData<EquippedWeaponTag>(playerEntity);
                    equipped.WeaponEntity = weaponEntity;
                    equipped.QuickSlotIndex = slotIndex;
                    state.EntityManager.SetComponentData(playerEntity, equipped);
                }
                else
                {
                    state.EntityManager.AddComponentData(playerEntity, new EquippedWeaponTag
                    {
                        WeaponEntity = weaponEntity,
                        QuickSlotIndex = slotIndex
                    });
                }
            }
        }

        /// <summary>
        /// Holsters all equipped weapons for player
        /// </summary>
        private void HolsterAllWeapons(ref SystemState state, Entity playerEntity)
        {
            // Find currently equipped weapon
            if (!state.EntityManager.HasComponent<EquippedWeaponTag>(playerEntity))
                return;

            var equipped = state.EntityManager.GetComponentData<EquippedWeaponTag>(playerEntity);
            Entity weaponEntity = equipped.WeaponEntity;

            if (weaponEntity == Entity.Null || !state.EntityManager.Exists(weaponEntity))
                return;

            // Holster weapon
            if (state.EntityManager.HasComponent<WeaponStateData>(weaponEntity))
            {
                var weaponState = state.EntityManager.GetComponentData<WeaponStateData>(weaponEntity);
                weaponState.IsHolstered = true;
                state.EntityManager.SetComponentData(weaponEntity, weaponState);
            }
        }
    }
}
