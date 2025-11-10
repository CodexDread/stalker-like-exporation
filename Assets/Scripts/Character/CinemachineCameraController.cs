using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Cinemachine;

namespace ZoneSurvival.Character
{
    /// <summary>
    /// MonoBehaviour bridge between ECS camera data and Cinemachine
    /// Reads CinemachineCameraData from ECS and applies values to Cinemachine virtual camera
    ///
    /// This hybrid approach gives us:
    /// - ECS performance for calculations
    /// - Cinemachine features for camera feel (damping, shake, noise)
    /// - Weight/inertia effects through Cinemachine's systems
    ///
    /// Attach this to a GameObject with a CinemachineVirtualCamera
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CinemachineCameraController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player entity that owns this camera")]
        public GameObject playerEntity;

        [Header("Cinemachine Components (Auto-assigned)")]
        private CinemachineVirtualCamera virtualCamera;
        private CinemachineBasicMultiChannelPerlin noiseComponent;
        private CinemachineImpulseSource impulseSource;

        [Header("Camera Target")]
        [Tooltip("Transform that the camera follows (usually at player's eye level)")]
        public Transform cameraTarget;

        [Header("Weight & Inertia Settings")]
        [Tooltip("Enable weight-based camera lag")]
        public bool enableWeightEffects = true;

        [Tooltip("Enable procedural camera noise (breathing, idle sway)")]
        public bool enableProceduralNoise = true;

        [Header("Debug")]
        public bool showDebugInfo = false;

        // ECS references
        private Entity playerEntityRef;
        private EntityManager entityManager;
        private bool isInitialized = false;

        // Cached values
        private float currentPitch = 0f;
        private float currentYaw = 0f;
        private CinemachineCameraData cachedCameraData;

        void Start()
        {
            // Get Cinemachine components
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                Debug.LogError("CinemachineCameraController: No CinemachineVirtualCamera found!");
                enabled = false;
                return;
            }

            // Add noise component if it doesn't exist
            noiseComponent = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noiseComponent == null)
            {
                noiseComponent = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }

            // Add impulse source for camera shake
            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (impulseSource == null)
            {
                impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            }

            // Create camera target if it doesn't exist
            if (cameraTarget == null)
            {
                GameObject targetObj = new GameObject("CameraTarget");
                cameraTarget = targetObj.transform;
                targetObj.transform.SetParent(playerEntity.transform);
                targetObj.transform.localPosition = Vector3.zero; // Will be set by offset
            }

            // Set virtual camera to follow the target
            virtualCamera.Follow = cameraTarget;
            virtualCamera.LookAt = null; // We handle rotation manually

            // Initialize ECS connection
            InitializeECSConnection();
        }

        void InitializeECSConnection()
        {
            if (playerEntity == null)
            {
                Debug.LogError("CinemachineCameraController: No player entity assigned!");
                enabled = false;
                return;
            }

            // Get World and EntityManager
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                Debug.LogError("CinemachineCameraController: No ECS World found!");
                enabled = false;
                return;
            }

            entityManager = world.EntityManager;

            // Try to get the entity reference from the GameObject
            var entityAuthoring = playerEntity.GetComponent<Unity.Entities.Conversion.GameObjectEntity>();
            if (entityAuthoring != null && entityAuthoring.Entity != Entity.Null)
            {
                playerEntityRef = entityAuthoring.Entity;
                isInitialized = true;
            }
            else
            {
                Debug.LogWarning("CinemachineCameraController: Entity not yet converted. Will retry...");
                Invoke(nameof(RetryECSConnection), 0.5f);
            }
        }

        void RetryECSConnection()
        {
            InitializeECSConnection();
        }

        void LateUpdate()
        {
            if (!isInitialized || playerEntityRef == Entity.Null)
                return;

            // Check if entity still exists and has camera component
            if (!entityManager.Exists(playerEntityRef) ||
                !entityManager.HasComponent<CinemachineCameraData>(playerEntityRef))
            {
                return;
            }

            // Read camera data from ECS
            cachedCameraData = entityManager.GetComponentData<CinemachineCameraData>(playerEntityRef);

            // Apply rotation to camera target
            ApplyCameraRotation();

            // Apply camera offset
            ApplyCameraOffset();

            // Apply FOV
            ApplyFOV();

            // Apply weight-based damping
            if (enableWeightEffects)
            {
                ApplyWeightDamping();
            }

            // Apply procedural noise (breathing, idle sway)
            if (enableProceduralNoise && cachedCameraData.EnableProceduralEffects)
            {
                ApplyProceduralNoise();
            }

            // Process camera shake requests
            ProcessCameraShake();
        }

        /// <summary>
        /// Apply pitch and yaw rotation to camera target transform
        /// </summary>
        void ApplyCameraRotation()
        {
            currentPitch = cachedCameraData.Pitch;
            currentYaw = cachedCameraData.Yaw;

            // Apply rotation to camera target
            // Yaw is applied to character body by ECS system
            // Pitch is applied to camera target here
            Quaternion pitchRotation = Quaternion.Euler(currentPitch, 0f, 0f);
            Quaternion yawRotation = Quaternion.Euler(0f, currentYaw, 0f);

            // Camera target inherits yaw from parent (player entity)
            // We only apply pitch locally
            cameraTarget.localRotation = pitchRotation;

            // Add recoil offset if present
            if (math.abs(cachedCameraData.RecoilYawAmount) > 0.01f)
            {
                // Apply yaw recoil as a local rotation offset
                Quaternion recoilOffset = Quaternion.Euler(0f, cachedCameraData.RecoilYawAmount, 0f);
                cameraTarget.localRotation *= recoilOffset;
            }
        }

        /// <summary>
        /// Update camera target position based on offset and stance
        /// </summary>
        void ApplyCameraOffset()
        {
            cameraTarget.localPosition = new Vector3(
                cachedCameraData.CameraOffset.x,
                cachedCameraData.CameraOffset.y,
                cachedCameraData.CameraOffset.z
            );
        }

        /// <summary>
        /// Apply field of view to virtual camera
        /// </summary>
        void ApplyFOV()
        {
            virtualCamera.m_Lens.FieldOfView = cachedCameraData.CurrentFOV;
        }

        /// <summary>
        /// Apply weight-based camera damping for inertia feeling
        /// Heavier load = more camera lag
        /// </summary>
        void ApplyWeightDamping()
        {
            // Get transposer component (handles position damping)
            var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                // Apply damping to camera position follow
                // Higher damping = more lag
                transposer.m_XDamping = cachedCameraData.CurrentRotationDamping * 0.5f;
                transposer.m_YDamping = cachedCameraData.CurrentRotationDamping * 0.5f;
                transposer.m_ZDamping = cachedCameraData.CurrentRotationDamping * 0.5f;
            }

            // Apply damping to camera rotation (via composer or aim)
            var composer = virtualCamera.GetCinemachineComponent<CinemachineComposer>();
            if (composer != null)
            {
                composer.m_HorizontalDamping = cachedCameraData.CurrentRotationDamping;
                composer.m_VerticalDamping = cachedCameraData.CurrentRotationDamping;
            }

            // For POV (first-person), we don't use Composer, rotation is instant
            // Instead, we can smooth the cameraTarget rotation itself
            // This is already handled by ECS damping calculation
        }

        /// <summary>
        /// Apply procedural noise for breathing and idle sway
        /// Creates subtle camera movement for realism
        /// </summary>
        void ApplyProceduralNoise()
        {
            if (noiseComponent == null)
                return;

            // Base noise profile (idle sway)
            float baseAmplitude = cachedCameraData.IdleSwayAmount * cachedCameraData.EffectsIntensity;

            // Add breathing
            float breathingInfluence = Mathf.Sin(cachedCameraData.CurrentBreathPhase) * cachedCameraData.BreathingAmplitude;
            float totalAmplitude = baseAmplitude + Mathf.Abs(breathingInfluence) * 0.5f;

            // Set noise amplitude
            noiseComponent.m_AmplitudeGain = totalAmplitude;

            // Frequency based on breathing rate
            noiseComponent.m_FrequencyGain = cachedCameraData.BreathingFrequency * 0.5f;

            // Use a noise profile (you'll need to assign this in the Inspector)
            // Common choice: "6D Shake" or "Handheld_normal" from Cinemachine presets
            if (noiseComponent.m_NoiseProfile == null)
            {
                Debug.LogWarning("CinemachineCameraController: No noise profile assigned! Assign a NoiseSettings asset in the Inspector.");
            }
        }

        /// <summary>
        /// Process camera shake requests from ECS
        /// Triggers Cinemachine impulses for impacts, explosions, etc.
        /// </summary>
        void ProcessCameraShake()
        {
            // Check if there's a shake request
            if (entityManager.HasComponent<CameraShakeRequest>(playerEntityRef))
            {
                var shakeRequest = entityManager.GetComponentData<CameraShakeRequest>(playerEntityRef);

                // Trigger Cinemachine impulse
                if (impulseSource != null)
                {
                    // Set impulse properties
                    impulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeRequest.Amplitude;
                    impulseSource.m_ImpulseDefinition.m_FrequencyGain = shakeRequest.Frequency;
                    impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = shakeRequest.Duration;

                    // Generate directional impulse
                    Vector3 direction = new Vector3(
                        shakeRequest.Direction.x,
                        shakeRequest.Direction.y,
                        shakeRequest.Direction.z
                    );

                    if (direction.sqrMagnitude < 0.01f)
                    {
                        // No specific direction, use random
                        impulseSource.GenerateImpulse();
                    }
                    else
                    {
                        // Directional impulse (for impacts)
                        impulseSource.GenerateImpulse(direction.normalized);
                    }
                }

                // ECS system will remove the component after reading
            }

            // Check for landing shake trigger
            if (cachedCameraData.TriggerLandingShake)
            {
                // Trigger landing impact shake
                // (Already handled by CameraShakeRequest in ECS)
                // This flag is just for debugging/additional effects
            }
        }

        void OnGUI()
        {
            if (!showDebugInfo)
                return;

            GUILayout.BeginArea(new Rect(10, 200, 300, 200));
            GUILayout.Label("=== CINEMACHINE CAMERA DEBUG ===");
            GUILayout.Label($"Pitch: {currentPitch:F1}°");
            GUILayout.Label($"Yaw: {currentYaw:F1}°");
            GUILayout.Label($"FOV: {cachedCameraData.CurrentFOV:F1}");
            GUILayout.Label($"Damping: {cachedCameraData.CurrentRotationDamping:F2}");
            GUILayout.Label($"Recoil Pitch: {cachedCameraData.RecoilPitchAmount:F3}");
            GUILayout.Label($"Recoil Yaw: {cachedCameraData.RecoilYawAmount:F3}");
            GUILayout.Label($"Breathing Phase: {cachedCameraData.CurrentBreathPhase:F2}");
            GUILayout.Label($"Noise Amplitude: {noiseComponent?.m_AmplitudeGain:F3}");
            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            // Cleanup camera target if we created it
            if (cameraTarget != null && cameraTarget.gameObject.name == "CameraTarget")
            {
                Destroy(cameraTarget.gameObject);
            }
        }
    }
}
