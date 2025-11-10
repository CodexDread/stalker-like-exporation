using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Interaction
{
    /// <summary>
    /// Executes interactions when player presses interact button
    /// Handles timed interactions (hold to interact)
    /// Triggers interaction events for other systems to process
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(InteractionDetectionSystem))]
    public partial struct InteractionExecutionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var interactor in SystemAPI.Query<RefRW<InteractorData>>())
            {
                // Update cooldown
                if (interactor.ValueRW.CooldownTimer > 0f)
                {
                    interactor.ValueRW.CooldownTimer -= deltaTime;
                }

                // Check if currently interacting with something
                if (interactor.ValueRO.InteractingWith != Entity.Null)
                {
                    // Continue ongoing interaction
                    if (state.EntityManager.Exists(interactor.ValueRO.InteractingWith))
                    {
                        var interactable = state.EntityManager.GetComponentData<InteractableTag>(
                            interactor.ValueRO.InteractingWith);

                        if (interactable.InteractionTime > 0f)
                        {
                            // Timed interaction
                            if (interactor.ValueRO.InteractHeld)
                            {
                                interactor.ValueRW.InteractionProgress += deltaTime / interactable.InteractionTime;

                                // Update interactable progress
                                interactable.InteractionProgress = interactor.ValueRO.InteractionProgress;
                                state.EntityManager.SetComponentData(interactor.ValueRO.InteractingWith, interactable);

                                // Check if completed
                                if (interactor.ValueRW.InteractionProgress >= 1.0f)
                                {
                                    CompleteInteraction(ref state, ref interactor.ValueRW, interactable);
                                }
                            }
                            else
                            {
                                // Released button - cancel interaction
                                CancelInteraction(ref state, ref interactor.ValueRW);
                            }
                        }
                    }
                    else
                    {
                        // Entity was destroyed - cancel interaction
                        CancelInteraction(ref state, ref interactor.ValueRW);
                    }
                }
                // Check for new interaction
                else if (interactor.ValueRO.InteractPressed &&
                         interactor.ValueRO.CurrentTarget != Entity.Null &&
                         interactor.ValueRO.CooldownTimer <= 0f)
                {
                    if (state.EntityManager.Exists(interactor.ValueRO.CurrentTarget))
                    {
                        var interactable = state.EntityManager.GetComponentData<InteractableTag>(
                            interactor.ValueRO.CurrentTarget);

                        if (interactable.IsEnabled && !interactable.IsBeingInteracted)
                        {
                            StartInteraction(ref state, ref interactor.ValueRW, interactable,
                                interactor.ValueRO.CurrentTarget);
                        }
                    }
                }
            }
        }

        private void StartInteraction(ref SystemState state, ref InteractorData interactor,
            InteractableTag interactable, Entity targetEntity)
        {
            interactor.InteractingWith = targetEntity;
            interactor.InteractionProgress = 0f;

            // Update interactable state
            interactable.IsBeingInteracted = true;
            interactable.InteractingEntity = interactor.Entity; // Store who's interacting
            interactable.InteractionProgress = 0f;
            state.EntityManager.SetComponentData(targetEntity, interactable);

            // If instant interaction, complete immediately
            if (interactable.InteractionTime <= 0f)
            {
                interactor.InteractionProgress = 1.0f;
                CompleteInteraction(ref state, ref interactor, interactable);
            }
        }

        private void CompleteInteraction(ref SystemState state, ref InteractorData interactor,
            InteractableTag interactable)
        {
            Entity targetEntity = interactor.InteractingWith;

            // Reset interactor state
            interactor.InteractingWith = Entity.Null;
            interactor.InteractionProgress = 0f;
            interactor.CooldownTimer = interactor.InteractionCooldown;

            // Reset interactable state
            interactable.IsBeingInteracted = false;
            interactable.InteractingEntity = Entity.Null;
            interactable.InteractionProgress = 0f;

            if (state.EntityManager.Exists(targetEntity))
            {
                state.EntityManager.SetComponentData(targetEntity, interactable);

                // Trigger interaction event based on type
                // This will be handled by specific systems (pickup, loot, etc.)
                switch (interactable.Type)
                {
                    case InteractionType.Pickup:
                        // Add pickup request tag (processed by ItemPickupSystem)
                        if (!state.EntityManager.HasComponent<PickupRequestTag>(targetEntity))
                        {
                            state.EntityManager.AddComponent<PickupRequestTag>(targetEntity);
                        }
                        break;

                    // Other interaction types handled by their respective systems
                    case InteractionType.Open:
                    case InteractionType.Use:
                    case InteractionType.Talk:
                    case InteractionType.Loot:
                    case InteractionType.Repair:
                    case InteractionType.Craft:
                    case InteractionType.Trade:
                        // Future implementation
                        break;
                }
            }
        }

        private void CancelInteraction(ref SystemState state, ref InteractorData interactor)
        {
            Entity targetEntity = interactor.InteractingWith;

            if (state.EntityManager.Exists(targetEntity))
            {
                var interactable = state.EntityManager.GetComponentData<InteractableTag>(targetEntity);
                interactable.IsBeingInteracted = false;
                interactable.InteractingEntity = Entity.Null;
                interactable.InteractionProgress = 0f;
                state.EntityManager.SetComponentData(targetEntity, interactable);
            }

            interactor.InteractingWith = Entity.Null;
            interactor.InteractionProgress = 0f;
        }
    }

    /// <summary>
    /// Tag component added to items that need to be picked up
    /// Removed after pickup is processed
    /// </summary>
    public struct PickupRequestTag : IComponentData { }
}
