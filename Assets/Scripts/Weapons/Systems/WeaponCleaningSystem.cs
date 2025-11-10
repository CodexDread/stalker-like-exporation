using Unity.Entities;
using Unity.Mathematics;
using ZoneSurvival.Items;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Handles weapon cleaning and maintenance
    /// - Restores weapon condition using cleaning kits
    /// - Restores part condition
    /// - Resets shots since cleaning counter
    ///
    /// Usage:
    /// 1. Add CleaningRequest component to weapon entity
    /// 2. System processes request, consumes cleaning kit, restores condition
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct WeaponCleaningSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Process cleaning requests
            foreach (var (cleanRequest, weaponState, itemData, entity) in
                     SystemAPI.Query<RefRO<CleaningRequest>, RefRW<WeaponStateData>, RefRW<ItemData>>()
                     .WithEntityAccess())
            {
                ProcessCleaningRequest(ref state, entity, cleanRequest.ValueRO,
                    ref weaponState.ValueRW, ref itemData.ValueRW);

                // Remove request (processed)
                state.EntityManager.RemoveComponent<CleaningRequest>(entity);
            }
        }

        /// <summary>
        /// Processes a cleaning request
        /// </summary>
        private void ProcessCleaningRequest(ref SystemState state, Entity weaponEntity,
            CleaningRequest request, ref WeaponStateData weaponState, ref ItemData itemData)
        {
            // Validate cleaning kit entity
            if (!state.EntityManager.Exists(request.CleaningKitEntity))
            {
                AddCleaningResult(ref state, weaponEntity, false, "Cleaning kit not found");
                return;
            }

            // Get cleaning kit data
            if (!state.EntityManager.HasComponent<CleaningKitData>(request.CleaningKitEntity))
            {
                AddCleaningResult(ref state, weaponEntity, false, "Invalid cleaning kit");
                return;
            }

            var cleaningKit = state.EntityManager.GetComponentData<CleaningKitData>(request.CleaningKitEntity);

            // Check if kit has uses left
            if (cleaningKit.UsesRemaining <= 0)
            {
                AddCleaningResult(ref state, weaponEntity, false, "Cleaning kit empty");
                return;
            }

            // Restore overall weapon condition
            float conditionRestored = cleaningKit.ConditionRestored;
            float oldCondition = itemData.Condition;
            itemData.Condition = math.min(itemData.Condition + conditionRestored, 1.0f);
            float actualRestored = itemData.Condition - oldCondition;

            // Reset shots since cleaning
            weaponState.ShotsSinceCleaning = 0;

            // Restore part conditions (if applicable)
            if (cleaningKit.RestoresPartCondition)
            {
                RestorePartConditions(ref state, weaponEntity, cleaningKit.PartConditionRestored);
            }

            // Consume cleaning kit use
            cleaningKit.UsesRemaining--;
            state.EntityManager.SetComponentData(request.CleaningKitEntity, cleaningKit);

            // Destroy kit if no uses left
            if (cleaningKit.UsesRemaining <= 0)
            {
                state.EntityManager.DestroyEntity(request.CleaningKitEntity);
            }

            // Add success result
            AddCleaningResult(ref state, weaponEntity, true,
                $"Restored {actualRestored:P0} condition");
        }

        /// <summary>
        /// Restores condition of all attached parts
        /// </summary>
        private void RestorePartConditions(ref SystemState state, Entity weaponEntity, float amount)
        {
            if (!state.EntityManager.HasBuffer<WeaponPartElement>(weaponEntity))
                return;

            var partsBuffer = state.EntityManager.GetBuffer<WeaponPartElement>(weaponEntity);

            foreach (var partElement in partsBuffer)
            {
                Entity partEntity = partElement.PartEntity;

                if (!state.EntityManager.Exists(partEntity))
                    continue;

                if (!state.EntityManager.HasComponent<WeaponPartData>(partEntity))
                    continue;

                var partData = state.EntityManager.GetComponentData<WeaponPartData>(partEntity);

                // Restore part condition
                partData.Condition = math.min(partData.Condition + amount, partData.MaxCondition);

                state.EntityManager.SetComponentData(partEntity, partData);
            }
        }

        /// <summary>
        /// Adds cleaning result for UI feedback
        /// </summary>
        private void AddCleaningResult(ref SystemState state, Entity weaponEntity, bool success, string message)
        {
            if (state.EntityManager.HasComponent<CleaningResult>(weaponEntity))
            {
                state.EntityManager.RemoveComponent<CleaningResult>(weaponEntity);
            }

            state.EntityManager.AddComponentData(weaponEntity, new CleaningResult
            {
                Success = success,
                Message = new Unity.Collections.FixedString128Bytes(message)
            });
        }
    }

    /// <summary>
    /// Component for cleaning kit items
    /// </summary>
    public struct CleaningKitData : IComponentData
    {
        public int UsesRemaining;            // How many times can be used
        public int MaxUses;                  // Maximum uses when new
        public float ConditionRestored;      // How much condition restored (0.0-1.0)
        public bool RestoresPartCondition;   // Also restores parts?
        public float PartConditionRestored;  // How much part condition restored

        // Quality tiers
        public CleaningKitQuality Quality;
    }

    public enum CleaningKitQuality : byte
    {
        Basic = 0,       // 5 uses, restores 0.1 (10%) condition
        Standard = 1,    // 10 uses, restores 0.15 (15%) condition
        Advanced = 2,    // 15 uses, restores 0.25 (25%) condition + parts
        Professional = 3 // 20 uses, restores 0.4 (40%) condition + parts
    }

    /// <summary>
    /// Request component - add to weapon to clean it
    /// </summary>
    public struct CleaningRequest : IComponentData
    {
        public Entity CleaningKitEntity;     // Which cleaning kit to use
    }

    /// <summary>
    /// Result component - added after cleaning
    /// </summary>
    public struct CleaningResult : IComponentData
    {
        public bool Success;
        public Unity.Collections.FixedString128Bytes Message;
    }

    /// <summary>
    /// Authoring component for cleaning kits
    /// </summary>
    public class CleaningKitAuthoring : UnityEngine.MonoBehaviour
    {
        [UnityEngine.Header("Cleaning Kit Properties")]
        public CleaningKitQuality quality = CleaningKitQuality.Standard;

        [UnityEngine.Header("Manual Configuration")]
        public int maxUses = 10;
        public float conditionRestored = 0.15f;
        public bool restoresPartCondition = false;
        public float partConditionRestored = 0.05f;

        class Baker : Baker<CleaningKitAuthoring>
        {
            public override void Bake(CleaningKitAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Apply preset or use manual values
                CleaningKitData data = GetPresetOrManual(authoring);

                AddComponent(entity, data);

                // Also add ItemData for inventory
                AddComponent(entity, new ItemData
                {
                    ItemID = 5000 + (int)authoring.quality,
                    ItemName = new Unity.Collections.FixedString64Bytes($"{authoring.quality} Cleaning Kit"),
                    Category = ItemCategory.Tools,
                    Rarity = ItemRarity.Common,
                    Weight = 0.2f,
                    GridWidth = 1,
                    GridHeight = 1,
                    IsStackable = true,
                    MaxStackSize = 3,
                    CurrentStackSize = 1,
                    BaseValue = 1000 * ((int)authoring.quality + 1),
                    HasCondition = false,
                    Condition = 1.0f
                });
            }

            private CleaningKitData GetPresetOrManual(CleaningKitAuthoring authoring)
            {
                CleaningKitData data = new CleaningKitData
                {
                    Quality = authoring.quality
                };

                // Apply preset based on quality
                switch (authoring.quality)
                {
                    case CleaningKitQuality.Basic:
                        data.MaxUses = 5;
                        data.UsesRemaining = 5;
                        data.ConditionRestored = 0.1f;
                        data.RestoresPartCondition = false;
                        data.PartConditionRestored = 0f;
                        break;

                    case CleaningKitQuality.Standard:
                        data.MaxUses = 10;
                        data.UsesRemaining = 10;
                        data.ConditionRestored = 0.15f;
                        data.RestoresPartCondition = false;
                        data.PartConditionRestored = 0f;
                        break;

                    case CleaningKitQuality.Advanced:
                        data.MaxUses = 15;
                        data.UsesRemaining = 15;
                        data.ConditionRestored = 0.25f;
                        data.RestoresPartCondition = true;
                        data.PartConditionRestored = 0.1f;
                        break;

                    case CleaningKitQuality.Professional:
                        data.MaxUses = 20;
                        data.UsesRemaining = 20;
                        data.ConditionRestored = 0.4f;
                        data.RestoresPartCondition = true;
                        data.PartConditionRestored = 0.15f;
                        break;
                }

                // Override with manual if specified
                if (authoring.maxUses != data.MaxUses)
                {
                    data.MaxUses = authoring.maxUses;
                    data.UsesRemaining = authoring.maxUses;
                }
                if (authoring.conditionRestored != data.ConditionRestored)
                    data.ConditionRestored = authoring.conditionRestored;
                if (authoring.restoresPartCondition != data.RestoresPartCondition)
                    data.RestoresPartCondition = authoring.restoresPartCondition;
                if (authoring.partConditionRestored != data.PartConditionRestored)
                    data.PartConditionRestored = authoring.partConditionRestored;

                return data;
            }
        }
    }
}
