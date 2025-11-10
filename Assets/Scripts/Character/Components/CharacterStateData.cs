using Unity.Entities;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Character movement state machine
    /// States: Idle, Walking, Sprinting, Crouching, Prone
    /// Based on GDD.md control specifications
    /// </summary>
    public enum MovementState : byte
    {
        Idle = 0,
        Walking = 1,
        Sprinting = 2,
        Crouching = 3,
        Prone = 4
    }

    public struct CharacterStateData : IComponentData
    {
        public MovementState CurrentState;
        public MovementState PreviousState;

        // State properties
        public float CurrentSpeed;
        public float CurrentHeight; // For capsule height adjustment
    }
}
