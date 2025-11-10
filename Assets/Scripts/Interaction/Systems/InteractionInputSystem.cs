using Unity.Entities;
using UnityEngine;

namespace ZoneSurvival.Interaction
{
    /// <summary>
    /// Captures interaction input from player
    /// Updates InteractorData with button press state
    /// Uses E key by default (configurable)
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class InteractionInputSystem : SystemBase
    {
        private bool wasInteractPressed;

        protected override void OnUpdate()
        {
            // Capture interact button (E key by default)
            bool interactPressed = Input.GetKey(KeyCode.E);
            bool interactPressedThisFrame = interactPressed && !wasInteractPressed;
            wasInteractPressed = interactPressed;

            // Update all interactors
            foreach (var interactor in SystemAPI.Query<RefRW<InteractorData>>())
            {
                interactor.ValueRW.InteractPressed = interactPressedThisFrame;
                interactor.ValueRW.InteractHeld = interactPressed;
            }
        }
    }
}
