using Unity.Entities;
using Unity.Collections;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Handles runtime weapon part attachment and detachment
    /// - Validates compatibility before attaching
    /// - Updates weapon stats after changes
    /// - Manages part ownership and inventory
    ///
    /// Usage:
    /// 1. Add PartAttachRequest or PartDetachRequest component to weapon entity
    /// 2. System processes request and modifies weapon parts buffer
    /// 3. WeaponStatsCalculationSystem automatically recalculates stats
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(WeaponStatsCalculationSystem))]
    public partial struct WeaponPartAttachmentSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            // Process attachment requests
            foreach (var (attachRequest, partsBuffer, slotsBuffer, entity) in
                     SystemAPI.Query<RefRO<PartAttachRequest>, DynamicBuffer<WeaponPartElement>, DynamicBuffer<WeaponPartSlotDefinition>>()
                     .WithEntityAccess())
            {
                bool success = ProcessAttachRequest(ref state, entity, attachRequest.ValueRO,
                    partsBuffer, slotsBuffer);

                // Remove request (processed)
                state.EntityManager.RemoveComponent<PartAttachRequest>(entity);

                // Add result component for UI feedback
                state.EntityManager.AddComponentData(entity, new PartAttachResult
                {
                    Success = success,
                    PartEntity = attachRequest.ValueRO.PartEntity,
                    FailureReason = success ? PartAttachFailure.None : PartAttachFailure.Incompatible
                });
            }

            // Process detachment requests
            foreach (var (detachRequest, partsBuffer, entity) in
                     SystemAPI.Query<RefRO<PartDetachRequest>, DynamicBuffer<WeaponPartElement>>()
                     .WithEntityAccess())
            {
                bool success = ProcessDetachRequest(ref state, entity, detachRequest.ValueRO, partsBuffer);

                // Remove request (processed)
                state.EntityManager.RemoveComponent<PartDetachRequest>(entity);

                // Add result component for UI feedback
                state.EntityManager.AddComponentData(entity, new PartDetachResult
                {
                    Success = success,
                    PartEntity = detachRequest.ValueRO.PartEntity
                });
            }
        }

        /// <summary>
        /// Processes part attachment request
        /// Returns true if successful, false if incompatible
        /// </summary>
        private bool ProcessAttachRequest(ref SystemState state, Entity weaponEntity,
            PartAttachRequest request, DynamicBuffer<WeaponPartElement> partsBuffer,
            DynamicBuffer<WeaponPartSlotDefinition> slotsBuffer)
        {
            Entity partEntity = request.PartEntity;

            // Validate part entity exists
            if (!state.EntityManager.Exists(partEntity))
                return false;

            // Get part data
            if (!state.EntityManager.HasComponent<WeaponPartData>(partEntity))
                return false;

            var partData = state.EntityManager.GetComponentData<WeaponPartData>(partEntity);

            // Check compatibility
            bool isCompatible = false;
            WeaponPartSlotDefinition compatibleSlot = default;

            for (int i = 0; i < slotsBuffer.Length; i++)
            {
                var slot = slotsBuffer[i];

                // Check if part type matches slot type
                if (slot.SlotType == partData.PartType)
                {
                    // Check mount type compatibility
                    if (slot.RequiredMount == PartMountType.Universal ||
                        slot.RequiredMount == partData.MountType)
                    {
                        // Check if slot is already full
                        int currentCount = CountPartsOfType(partsBuffer, partData.PartType);
                        if (currentCount < slot.MaxCount)
                        {
                            isCompatible = true;
                            compatibleSlot = slot;
                            break;
                        }
                    }
                }
            }

            if (!isCompatible)
                return false;

            // Remove existing part if replacing (and slot only allows 1)
            if (compatibleSlot.MaxCount == 1)
            {
                for (int i = partsBuffer.Length - 1; i >= 0; i--)
                {
                    if (partsBuffer[i].SlotType == partData.PartType)
                    {
                        // Detach old part
                        Entity oldPart = partsBuffer[i].PartEntity;
                        partsBuffer.RemoveAt(i);

                        // Return old part to inventory (TODO: integrate with inventory system)
                    }
                }
            }

            // Attach new part
            partsBuffer.Add(new WeaponPartElement
            {
                PartEntity = partEntity,
                SlotType = partData.PartType,
                IsRequired = compatibleSlot.IsRequired
            });

            // Trigger visual model update
            if (!state.EntityManager.HasComponent<WeaponModelUpdateRequest>(weaponEntity))
            {
                state.EntityManager.AddComponent<WeaponModelUpdateRequest>(weaponEntity);
            }

            return true;
        }

        /// <summary>
        /// Processes part detachment request
        /// Returns true if successful
        /// </summary>
        private bool ProcessDetachRequest(ref SystemState state, Entity weaponEntity,
            PartDetachRequest request, DynamicBuffer<WeaponPartElement> partsBuffer)
        {
            Entity partEntity = request.PartEntity;

            // Find part in buffer
            for (int i = 0; i < partsBuffer.Length; i++)
            {
                if (partsBuffer[i].PartEntity == partEntity)
                {
                    // Cannot remove required parts
                    if (partsBuffer[i].IsRequired)
                        return false;

                    // Remove part
                    partsBuffer.RemoveAt(i);

                    // Return part to inventory (TODO: integrate with inventory system)

                    // Trigger visual model update
                    if (!state.EntityManager.HasComponent<WeaponModelUpdateRequest>(weaponEntity))
                    {
                        state.EntityManager.AddComponent<WeaponModelUpdateRequest>(weaponEntity);
                    }

                    return true;
                }
            }

            return false; // Part not found
        }

        /// <summary>
        /// Counts how many parts of a specific type are attached
        /// </summary>
        private int CountPartsOfType(DynamicBuffer<WeaponPartElement> partsBuffer, WeaponPartType partType)
        {
            int count = 0;
            for (int i = 0; i < partsBuffer.Length; i++)
            {
                if (partsBuffer[i].SlotType == partType)
                    count++;
            }
            return count;
        }
    }

    /// <summary>
    /// Request component - add to weapon to attach a part
    /// </summary>
    public struct PartAttachRequest : IComponentData
    {
        public Entity PartEntity;            // Part to attach
        public WeaponPartType TargetSlot;    // Optional: specific slot to attach to
    }

    /// <summary>
    /// Request component - add to weapon to detach a part
    /// </summary>
    public struct PartDetachRequest : IComponentData
    {
        public Entity PartEntity;            // Part to detach
    }

    /// <summary>
    /// Result component - added after processing request
    /// UI can read this for feedback
    /// </summary>
    public struct PartAttachResult : IComponentData
    {
        public bool Success;
        public Entity PartEntity;
        public PartAttachFailure FailureReason;
    }

    public struct PartDetachResult : IComponentData
    {
        public bool Success;
        public Entity PartEntity;
    }

    public enum PartAttachFailure : byte
    {
        None = 0,
        Incompatible = 1,
        SlotFull = 2,
        PartNotFound = 3,
        RequiredPart = 4         // Cannot remove required parts
    }

    /// <summary>
    /// Request tag - added to weapon when parts change
    /// Visual model system processes this and updates 3D models
    /// </summary>
    public struct WeaponModelUpdateRequest : IComponentData { }
}
