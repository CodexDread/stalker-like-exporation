using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using ZoneSurvival.Interaction;

namespace ZoneSurvival.Items
{
    /// <summary>
    /// Authoring component for world items
    /// Place this on GameObjects that represent items in the world
    /// Converts to ECS entity with all necessary components for pickup
    /// </summary>
    public class WorldItemAuthoring : MonoBehaviour
    {
        [Header("Item Identification")]
        [Tooltip("Unique item type ID")]
        public int itemID = 1;

        [Tooltip("Display name shown to player")]
        public string itemName = "Item";

        [Header("Item Properties")]
        public ItemCategory category = ItemCategory.Junk;
        public ItemRarity rarity = ItemRarity.Common;
        public ItemUsageType usageType = ItemUsageType.None;

        [Header("Physical Properties")]
        [Tooltip("Weight in kilograms")]
        public float weight = 0.5f;

        [Tooltip("Width in inventory grid (1-4)")]
        [Range(1, 4)]
        public int gridWidth = 1;

        [Tooltip("Height in inventory grid (1-4)")]
        [Range(1, 4)]
        public int gridHeight = 1;

        [Header("Stack Properties")]
        public bool isStackable = false;

        [Tooltip("Maximum stack size (0 = infinite, 1 = not stackable)")]
        public int maxStackSize = 1;

        [Tooltip("How many items in this stack")]
        public int currentStackSize = 1;

        [Header("Value & Condition")]
        [Tooltip("Base price in Rubles")]
        public int baseValue = 100;

        public bool hasCondition = false;

        [Tooltip("Current condition (0.0 - 1.0)")]
        [Range(0f, 1f)]
        public float condition = 1.0f;

        [Header("Quest & Special")]
        public bool isQuestItem = false;

        [Header("World Interaction")]
        [Tooltip("Distance at which item can be picked up")]
        public float pickupRange = 2f;

        [Tooltip("Distance at which name appears above item")]
        public float nameDisplayDistance = 5f;

        [Tooltip("Offset for UI display (typically upward)")]
        public Vector3 uiOffset = new Vector3(0f, 0.5f, 0f);

        [Header("Interaction Settings")]
        [Tooltip("Time required to pick up item (0 = instant)")]
        public float interactionTime = 0f;

        class Baker : Baker<WorldItemAuthoring>
        {
            public override void Bake(WorldItemAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Add base item data
                AddComponent(entity, new ItemData
                {
                    ItemID = authoring.itemID,
                    ItemName = authoring.itemName,
                    Category = authoring.category,
                    Rarity = authoring.rarity,
                    UsageType = authoring.usageType,
                    Weight = authoring.weight,
                    GridWidth = authoring.gridWidth,
                    GridHeight = authoring.gridHeight,
                    IsStackable = authoring.isStackable,
                    MaxStackSize = authoring.maxStackSize,
                    CurrentStackSize = authoring.currentStackSize,
                    BaseValue = authoring.baseValue,
                    HasCondition = authoring.hasCondition,
                    Condition = authoring.condition,
                    MaxCondition = 1.0f,
                    IsQuestItem = authoring.isQuestItem,
                    IsExamined = false
                });

                // Add world item tag
                AddComponent(entity, new WorldItemTag
                {
                    IsPickupable = true,
                    PickupRange = authoring.pickupRange,
                    ShowNameUI = true,
                    NameDisplayDistance = authoring.nameDisplayDistance,
                    UIOffset = authoring.uiOffset
                });

                // Add interactable tag for pickup
                AddComponent(entity, new InteractableTag
                {
                    Type = InteractionType.Pickup,
                    InteractionRange = authoring.pickupRange,
                    InteractionTime = authoring.interactionTime,
                    IsEnabled = true,
                    RequiresInput = true,
                    IsBeingInteracted = false,
                    InteractionProgress = 0f,
                    InteractingEntity = Entity.Null
                });

                // Add UI data
                AddComponent(entity, new WorldItemUIData
                {
                    IsVisible = false,
                    DistanceToPlayer = 999f,
                    FadeStartDistance = authoring.nameDisplayDistance * 0.8f,
                    FadeEndDistance = authoring.nameDisplayDistance * 0.3f,
                    Alpha = 0f,
                    AlwaysFaceCamera = true,
                    UIWorldPosition = float3.zero
                });
            }
        }
    }
}
