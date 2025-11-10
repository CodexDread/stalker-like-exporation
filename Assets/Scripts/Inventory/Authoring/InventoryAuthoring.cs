using Unity.Entities;
using UnityEngine;

namespace ZoneSurvival.Inventory
{
    /// <summary>
    /// Authoring component for inventory
    /// Add to player character to give them an inventory
    /// Based on GDD.md Tetris-style inventory (line 1111)
    /// </summary>
    public class InventoryAuthoring : MonoBehaviour
    {
        [Header("Inventory Grid")]
        [Tooltip("Number of columns in inventory grid")]
        public int gridWidth = 10;

        [Tooltip("Number of rows in inventory grid")]
        public int gridHeight = 6;

        [Header("Weight Limits")]
        [Tooltip("Maximum carry weight (linked to character's EncumbranceData)")]
        public float maxWeight = 30f;

        [Header("Starting Currency")]
        [Tooltip("Starting money in Rubles (RU)")]
        public int startingCurrency = 1000;

        [Header("Quick Slots")]
        [Tooltip("Number of weapon/equipment quick slots (1-0 keys)")]
        public int weaponQuickSlots = 10;

        [Tooltip("Number of consumable quick slots (F1-F4 keys)")]
        public int consumableQuickSlots = 4;

        class Baker : Baker<InventoryAuthoring>
        {
            public override void Bake(InventoryAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                int totalSlots = authoring.gridWidth * authoring.gridHeight;

                // Add inventory data
                AddComponent(entity, new InventoryData
                {
                    GridWidth = authoring.gridWidth,
                    GridHeight = authoring.gridHeight,
                    CurrentWeight = 0f,
                    MaxWeight = authoring.maxWeight,
                    TotalSlots = totalSlots,
                    OccupiedSlots = 0,
                    FreeSlots = totalSlots,
                    Currency = authoring.startingCurrency
                });

                // Create inventory slot buffer
                var slotBuffer = AddBuffer<InventorySlotBuffer>(entity);
                for (int i = 0; i < totalSlots; i++)
                {
                    slotBuffer.Add(new InventorySlotBuffer
                    {
                        Slot = new InventorySlot
                        {
                            ItemEntity = Entity.Null,
                            ItemID = 0,
                            StackSize = 0,
                            IsOccupied = false,
                            OriginX = -1,
                            OriginY = -1
                        }
                    });
                }

                // Add quick slots data
                AddComponent(entity, new QuickSlotsData
                {
                    CurrentWeaponSlot = 0,
                    LastWeaponSlot = 0,
                    IsWeaponDrawn = false,
                    WeaponSwitchTime = 1.0f,
                    SwitchProgress = 0f
                });

                // Create weapon quick slot buffer (1-0 keys)
                var weaponSlots = AddBuffer<WeaponQuickSlotBuffer>(entity);
                for (int i = 0; i < authoring.weaponQuickSlots; i++)
                {
                    weaponSlots.Add(new WeaponQuickSlotBuffer
                    {
                        Slot = new QuickSlot
                        {
                            ItemEntity = Entity.Null,
                            ItemID = 0,
                            IsOccupied = false,
                            IsEquipped = false
                        }
                    });
                }

                // Create consumable quick slot buffer (F1-F4 keys)
                var consumableSlots = AddBuffer<ConsumableQuickSlotBuffer>(entity);
                for (int i = 0; i < authoring.consumableQuickSlots; i++)
                {
                    consumableSlots.Add(new ConsumableQuickSlotBuffer
                    {
                        Slot = new QuickSlot
                        {
                            ItemEntity = Entity.Null,
                            ItemID = 0,
                            IsOccupied = false,
                            IsEquipped = false
                        }
                    });
                }
            }
        }
    }
}
