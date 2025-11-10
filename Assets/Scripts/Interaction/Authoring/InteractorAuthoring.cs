using Unity.Entities;
using UnityEngine;

namespace ZoneSurvival.Interaction
{
    /// <summary>
    /// Authoring component for entities that can interact (player)
    /// Add to player character to enable interaction with world objects
    /// </summary>
    public class InteractorAuthoring : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Maximum interaction distance")]
        public float interactionRange = 2f;

        [Tooltip("Raycast distance to detect interactables")]
        public float raycastDistance = 3f;

        [Tooltip("Cooldown between interactions")]
        public float interactionCooldown = 0.2f;

        class Baker : Baker<InteractorAuthoring>
        {
            public override void Bake(InteractorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // NOTE: Input now comes from PlayerInputData (unified input system)
                AddComponent(entity, new InteractorData
                {
                    CurrentTarget = Entity.Null,
                    InteractingWith = Entity.Null,
                    InteractionProgress = 0f,
                    InteractionRange = authoring.interactionRange,
                    RaycastDistance = authoring.raycastDistance,
                    InteractionCooldown = authoring.interactionCooldown,
                    CooldownTimer = 0f
                });
            }
        }
    }
}
