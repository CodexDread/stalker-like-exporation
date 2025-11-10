using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using ZoneSurvival.Items;
using ZoneSurvival.Character;

namespace ZoneSurvival.Inventory
{
    /// <summary>
    /// Manages inventory operations: adding, removing, moving items
    /// Handles Tetris-style grid placement and stacking
    /// Based on GDD.md inventory system (line 1111)
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct InventoryManagementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // This system processes inventory add/remove requests
            // Actual operations will be called by other systems (pickup, drop, etc.)
        }

        /// <summary>
        /// Attempts to add an item to inventory
        /// Returns true if successful, false if no space
        /// </summary>
        public static bool TryAddItem(ref SystemState state, Entity inventoryOwner, Entity itemEntity)
        {
            if (!state.EntityManager.HasComponent<InventoryData>(inventoryOwner))
                return false;

            if (!state.EntityManager.HasComponent<ItemData>(itemEntity))
                return false;

            var inventory = state.EntityManager.GetComponentData<InventoryData>(inventoryOwner);
            var item = state.EntityManager.GetComponentData<ItemData>(itemEntity);
            var slots = state.EntityManager.GetBuffer<InventorySlotBuffer>(inventoryOwner);

            // Check if item is stackable and already exists in inventory
            if (item.IsStackable && item.CurrentStackSize > 0)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    var slot = slots[i].Slot;
                    if (slot.IsOccupied && slot.ItemID == item.ItemID)
                    {
                        // Found existing stack - check if can add
                        var existingItem = state.EntityManager.GetComponentData<ItemData>(slot.ItemEntity);

                        if (existingItem.MaxStackSize == 0 ||
                            existingItem.CurrentStackSize + item.CurrentStackSize <= existingItem.MaxStackSize)
                        {
                            // Add to existing stack
                            existingItem.CurrentStackSize += item.CurrentStackSize;
                            state.EntityManager.SetComponentData(slot.ItemEntity, existingItem);

                            // Update slot
                            slot.StackSize = existingItem.CurrentStackSize;
                            slots[i] = new InventorySlotBuffer { Slot = slot };

                            // Destroy the picked up item entity
                            state.EntityManager.DestroyEntity(itemEntity);

                            return true;
                        }
                    }
                }
            }

            // Find empty space for item
            int2 position = FindEmptySpace(slots, inventory.GridWidth, inventory.GridHeight,
                item.GridWidth, item.GridHeight);

            if (position.x == -1)
                return false; // No space found

            // Place item in inventory
            PlaceItemInGrid(ref state, slots, itemEntity, item, position, inventory.GridWidth);

            // Update inventory weight
            inventory.CurrentWeight += item.Weight * item.CurrentStackSize;
            inventory.OccupiedSlots += item.GridWidth * item.GridHeight;
            inventory.FreeSlots = (inventory.GridWidth * inventory.GridHeight) - inventory.OccupiedSlots;
            state.EntityManager.SetComponentData(inventoryOwner, inventory);

            // Update encumbrance
            if (state.EntityManager.HasComponent<EncumbranceData>(inventoryOwner))
            {
                var encumbrance = state.EntityManager.GetComponentData<EncumbranceData>(inventoryOwner);
                encumbrance.CurrentWeight = inventory.CurrentWeight;
                state.EntityManager.SetComponentData(inventoryOwner, encumbrance);
            }

            return true;
        }

        /// <summary>
        /// Finds empty space in inventory grid for item
        /// Returns (-1, -1) if no space found
        /// </summary>
        private static int2 FindEmptySpace(DynamicBuffer<InventorySlotBuffer> slots,
            int gridWidth, int gridHeight, int itemWidth, int itemHeight)
        {
            // Try to find space for item
            for (int y = 0; y <= gridHeight - itemHeight; y++)
            {
                for (int x = 0; x <= gridWidth - itemWidth; x++)
                {
                    if (CanPlaceItem(slots, gridWidth, x, y, itemWidth, itemHeight))
                    {
                        return new int2(x, y);
                    }
                }
            }

            return new int2(-1, -1);
        }

        /// <summary>
        /// Checks if an item can be placed at given position
        /// </summary>
        private static bool CanPlaceItem(DynamicBuffer<InventorySlotBuffer> slots,
            int gridWidth, int x, int y, int itemWidth, int itemHeight)
        {
            for (int dy = 0; dy < itemHeight; dy++)
            {
                for (int dx = 0; dx < itemWidth; dx++)
                {
                    int index = (y + dy) * gridWidth + (x + dx);
                    if (index >= slots.Length || slots[index].Slot.IsOccupied)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Places item in inventory grid
        /// </summary>
        private static void PlaceItemInGrid(ref SystemState state, DynamicBuffer<InventorySlotBuffer> slots,
            Entity itemEntity, ItemData item, int2 position, int gridWidth)
        {
            // Mark all cells occupied by this item
            for (int dy = 0; dy < item.GridHeight; dy++)
            {
                for (int dx = 0; dx < item.GridWidth; dx++)
                {
                    int index = (position.y + dy) * gridWidth + (position.x + dx);

                    var slot = slots[index].Slot;
                    slot.IsOccupied = true;
                    slot.ItemEntity = itemEntity;
                    slot.ItemID = item.ItemID;
                    slot.StackSize = item.CurrentStackSize;
                    slot.OriginX = position.x;
                    slot.OriginY = position.y;

                    slots[index] = new InventorySlotBuffer { Slot = slot };
                }
            }
        }

        /// <summary>
        /// Removes item from inventory
        /// </summary>
        public static bool TryRemoveItem(ref SystemState state, Entity inventoryOwner, Entity itemEntity)
        {
            if (!state.EntityManager.HasComponent<InventoryData>(inventoryOwner))
                return false;

            var inventory = state.EntityManager.GetComponentData<InventoryData>(inventoryOwner);
            var slots = state.EntityManager.GetBuffer<InventorySlotBuffer>(inventoryOwner);
            var item = state.EntityManager.GetComponentData<ItemData>(itemEntity);

            // Find and clear item slots
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i].Slot;
                if (slot.ItemEntity == itemEntity)
                {
                    slot.IsOccupied = false;
                    slot.ItemEntity = Entity.Null;
                    slot.ItemID = 0;
                    slot.StackSize = 0;
                    slots[i] = new InventorySlotBuffer { Slot = slot };
                }
            }

            // Update inventory weight
            inventory.CurrentWeight -= item.Weight * item.CurrentStackSize;
            inventory.OccupiedSlots -= item.GridWidth * item.GridHeight;
            inventory.FreeSlots = (inventory.GridWidth * inventory.GridHeight) - inventory.OccupiedSlots;
            state.EntityManager.SetComponentData(inventoryOwner, inventory);

            // Update encumbrance
            if (state.EntityManager.HasComponent<EncumbranceData>(inventoryOwner))
            {
                var encumbrance = state.EntityManager.GetComponentData<EncumbranceData>(inventoryOwner);
                encumbrance.CurrentWeight = inventory.CurrentWeight;
                state.EntityManager.SetComponentData(inventoryOwner, encumbrance);
            }

            return true;
        }
    }
}
