using Unity.Entities;
using Unity.Mathematics;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Adjusts character capsule height based on movement state
    /// Handles smooth transitions between standing/crouching/prone
    /// Based on GDD.md movement states
    ///
    /// Heights:
    /// - Standing: 1.8m
    /// - Crouching: 1.2m
    /// - Prone: 0.5m
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CharacterMovementSystem))]
    public partial struct CapsuleHeightSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (physicsData, stateData, cameraData) in
                     SystemAPI.Query<RefRW<CharacterPhysicsData>, RefRO<CharacterStateData>,
                         RefRW<FirstPersonCameraData>>())
            {
                // Determine target height based on movement state
                float targetHeight = stateData.ValueRO.CurrentState switch
                {
                    MovementState.Prone => physicsData.ValueRO.ProneHeight,
                    MovementState.Crouching => physicsData.ValueRO.CrouchingHeight,
                    _ => physicsData.ValueRO.StandingHeight // Idle, Walking, Sprinting
                };

                physicsData.ValueRW.TargetHeight = targetHeight;

                // Smoothly interpolate current height to target
                if (math.abs(physicsData.ValueRO.CurrentHeight - targetHeight) > 0.01f)
                {
                    physicsData.ValueRW.CurrentHeight = math.lerp(
                        physicsData.ValueRO.CurrentHeight,
                        targetHeight,
                        deltaTime * physicsData.ValueRO.HeightTransitionSpeed
                    );
                }
                else
                {
                    physicsData.ValueRW.CurrentHeight = targetHeight;
                }

                // Update state height for other systems to reference
                stateData.ValueRW.CurrentHeight = physicsData.ValueRO.CurrentHeight;

                // Adjust camera offset to match height
                // Camera should be at eye level (slightly below top of capsule)
                float eyeHeightRatio = 0.9f; // Eyes at 90% of height
                float3 newCameraOffset = cameraData.ValueRO.CameraOffset;
                newCameraOffset.y = physicsData.ValueRO.CurrentHeight * eyeHeightRatio;
                cameraData.ValueRW.CameraOffset = newCameraOffset;
            }
        }
    }
}
