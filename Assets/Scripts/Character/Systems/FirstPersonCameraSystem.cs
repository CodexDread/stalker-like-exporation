using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// Handles first-person camera rotation and FOV
    /// Processes mouse input to rotate camera and character
    /// Based on GDD.md first-person shooter mechanics
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class FirstPersonCameraSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (cameraData, inputData, transform, state) in
                     SystemAPI.Query<RefRW<FirstPersonCameraData>, RefRO<PlayerInputData>, RefRW<LocalTransform>, RefRO<CharacterStateData>>())
            {
                // Apply mouse input to camera rotation
                cameraData.ValueRW.Yaw += inputData.ValueRO.LookInput.x * cameraData.ValueRO.MouseSensitivityX;
                cameraData.ValueRW.Pitch -= inputData.ValueRO.LookInput.y * cameraData.ValueRO.MouseSensitivityY;

                // Clamp pitch to prevent over-rotation
                cameraData.ValueRW.Pitch = math.clamp(
                    cameraData.ValueRW.Pitch,
                    cameraData.ValueRO.MinPitch,
                    cameraData.ValueRO.MaxPitch
                );

                // Wrap yaw to 0-360 range
                if (cameraData.ValueRW.Yaw >= 360f)
                    cameraData.ValueRW.Yaw -= 360f;
                else if (cameraData.ValueRW.Yaw < 0f)
                    cameraData.ValueRW.Yaw += 360f;

                // Calculate rotation quaternion
                quaternion yawRotation = quaternion.RotateY(math.radians(cameraData.ValueRO.Yaw));
                quaternion pitchRotation = quaternion.RotateX(math.radians(cameraData.ValueRO.Pitch));

                // Apply rotation to transform (yaw rotates the character body, pitch is camera-only)
                transform.ValueRW.Rotation = yawRotation;

                // Handle FOV changes based on movement state (sprinting increases FOV slightly)
                float targetFOV = state.ValueRO.CurrentState == MovementState.Sprinting
                    ? cameraData.ValueRO.SprintFOV
                    : cameraData.ValueRO.BaseFOV;

                cameraData.ValueRW.CurrentFOV = math.lerp(
                    cameraData.ValueRO.CurrentFOV,
                    targetFOV,
                    deltaTime * cameraData.ValueRO.FOVLerpSpeed
                );
            }
        }
    }
}
