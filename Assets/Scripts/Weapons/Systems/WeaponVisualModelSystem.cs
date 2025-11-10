using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;

namespace ZoneSurvival.Weapons
{
    /// <summary>
    /// Manages visual 3D models for weapons and their parts
    /// - Attaches/detaches part models dynamically
    /// - Updates weapon appearance when parts change
    /// - Handles model hierarchy and positioning
    ///
    /// Processes WeaponModelUpdateRequest tags
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class WeaponVisualModelSystem : SystemBase
    {
        // Cache of prefab ID -> GameObject prefab mappings
        private Dictionary<int, GameObject> prefabCache = new Dictionary<int, GameObject>();

        // Weapon entity -> root GameObject mapping
        private Dictionary<Entity, GameObject> weaponModels = new Dictionary<Entity, GameObject>();

        // Part entity -> spawned GameObject mapping
        private Dictionary<Entity, GameObject> partModels = new Dictionary<Entity, GameObject>();

        protected override void OnUpdate()
        {
            // Process model update requests
            Entities
                .WithAll<WeaponModelUpdateRequest>()
                .WithoutBurst()
                .ForEach((Entity entity, in WeaponStateData weaponState, DynamicBuffer<WeaponPartElement> partsBuffer) =>
                {
                    UpdateWeaponModel(entity, partsBuffer);

                    // Remove request (processed)
                    EntityManager.RemoveComponent<WeaponModelUpdateRequest>(entity);
                }).Run();

            // Update model positions for equipped weapons
            Entities
                .WithoutBurst()
                .ForEach((Entity entity, in WeaponStateData weaponState, in LocalTransform transform) =>
                {
                    if (weaponState.IsEquipped && !weaponState.IsHolstered)
                    {
                        UpdateModelTransform(entity, transform);
                    }
                }).Run();
        }

        /// <summary>
        /// Updates weapon's visual model based on attached parts
        /// </summary>
        private void UpdateWeaponModel(Entity weaponEntity, DynamicBuffer<WeaponPartElement> partsBuffer)
        {
            // Get or create weapon root model
            if (!weaponModels.TryGetValue(weaponEntity, out GameObject weaponRoot))
            {
                weaponRoot = CreateWeaponRootModel(weaponEntity);
                weaponModels[weaponEntity] = weaponRoot;
            }

            // Track which parts should be visible
            HashSet<Entity> activeParts = new HashSet<Entity>();
            foreach (var partElement in partsBuffer)
            {
                activeParts.Add(partElement.PartEntity);
            }

            // Remove models for detached parts
            List<Entity> partsToRemove = new List<Entity>();
            foreach (var kvp in partModels)
            {
                Entity partEntity = kvp.Key;
                if (!activeParts.Contains(partEntity))
                {
                    // Part was detached, destroy its model
                    if (kvp.Value != null)
                        GameObject.Destroy(kvp.Value);
                    partsToRemove.Add(partEntity);
                }
            }
            foreach (var partEntity in partsToRemove)
            {
                partModels.Remove(partEntity);
            }

            // Add/update models for attached parts
            foreach (var partElement in partsBuffer)
            {
                Entity partEntity = partElement.PartEntity;

                if (!EntityManager.Exists(partEntity))
                    continue;

                if (!EntityManager.HasComponent<WeaponPartData>(partEntity))
                    continue;

                var partData = EntityManager.GetComponentData<WeaponPartData>(partEntity);

                // Skip if part is not visible
                if (!partData.IsVisible)
                    continue;

                // Create or update part model
                if (!partModels.ContainsKey(partEntity))
                {
                    GameObject partModel = CreatePartModel(partData, weaponRoot);
                    if (partModel != null)
                    {
                        partModels[partEntity] = partModel;
                    }
                }
                else
                {
                    // Update existing part model (e.g., condition-based visuals)
                    UpdatePartModel(partModels[partEntity], partData);
                }
            }
        }

        /// <summary>
        /// Creates root GameObject for weapon
        /// </summary>
        private GameObject CreateWeaponRootModel(Entity weaponEntity)
        {
            GameObject root = new GameObject($"Weapon_{weaponEntity.Index}");

            // TODO: Load base weapon model from WeaponItemData
            // For now, create a simple placeholder

            return root;
        }

        /// <summary>
        /// Creates GameObject for a weapon part
        /// </summary>
        private GameObject CreatePartModel(WeaponPartData partData, GameObject weaponRoot)
        {
            // Get prefab from cache or load it
            if (!prefabCache.TryGetValue(partData.PrefabID, out GameObject prefab))
            {
                prefab = LoadPrefab(partData.PrefabID);
                if (prefab != null)
                {
                    prefabCache[partData.PrefabID] = prefab;
                }
            }

            if (prefab == null)
            {
                Debug.LogWarning($"No prefab found for part {partData.PartName} (ID: {partData.PrefabID})");
                return null;
            }

            // Instantiate part model as child of weapon root
            GameObject partModel = GameObject.Instantiate(prefab, weaponRoot.transform);
            partModel.name = $"Part_{partData.PartName}_{partData.PartType}";

            // Position part based on type
            PositionPartOnWeapon(partModel, partData.PartType, weaponRoot);

            return partModel;
        }

        /// <summary>
        /// Updates existing part model (e.g., for condition-based visuals)
        /// </summary>
        private void UpdatePartModel(GameObject partModel, WeaponPartData partData)
        {
            if (partModel == null)
                return;

            // Update visual based on condition
            // E.g., change material, add rust/wear, etc.

            // Example: Adjust material color based on condition
            var renderer = partModel.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Lerp between worn and pristine colors
                Color wornColor = new Color(0.5f, 0.5f, 0.5f); // Gray
                Color pristineColor = Color.white;
                Color currentColor = Color.Lerp(wornColor, pristineColor, partData.Condition);

                foreach (var mat in renderer.materials)
                {
                    mat.color = currentColor;
                }
            }
        }

        /// <summary>
        /// Positions part model on weapon based on part type
        /// Uses attachment points if available, otherwise default positions
        /// </summary>
        private void PositionPartOnWeapon(GameObject partModel, WeaponPartType partType, GameObject weaponRoot)
        {
            // Try to find specific attachment point
            Transform attachPoint = FindAttachmentPoint(weaponRoot, partType);

            if (attachPoint != null)
            {
                // Parent to attachment point
                partModel.transform.SetParent(attachPoint, false);
                partModel.transform.localPosition = Vector3.zero;
                partModel.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // Use default positions if no attachment point found
                Vector3 defaultPosition = GetDefaultPartPosition(partType);
                partModel.transform.localPosition = defaultPosition;
                partModel.transform.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Finds named attachment point on weapon model
        /// </summary>
        private Transform FindAttachmentPoint(GameObject weaponRoot, WeaponPartType partType)
        {
            string attachPointName = $"Attach_{partType}";
            Transform attachPoint = weaponRoot.transform.Find(attachPointName);

            // Try alternative names
            if (attachPoint == null)
            {
                switch (partType)
                {
                    case WeaponPartType.Scope:
                        attachPoint = weaponRoot.transform.Find("Attach_Optic") ??
                                     weaponRoot.transform.Find("RailTop");
                        break;
                    case WeaponPartType.Muzzle:
                        attachPoint = weaponRoot.transform.Find("Muzzle") ??
                                     weaponRoot.transform.Find("BarrelEnd");
                        break;
                    case WeaponPartType.Grip:
                        attachPoint = weaponRoot.transform.Find("Attach_Foregrip") ??
                                     weaponRoot.transform.Find("RailBottom");
                        break;
                    // Add more cases as needed
                }
            }

            return attachPoint;
        }

        /// <summary>
        /// Returns default local position for part type
        /// Used when no attachment point is defined
        /// </summary>
        private Vector3 GetDefaultPartPosition(WeaponPartType partType)
        {
            switch (partType)
            {
                case WeaponPartType.Barrel:
                    return new Vector3(0, 0, 0.3f);
                case WeaponPartType.Stock:
                    return new Vector3(0, 0, -0.3f);
                case WeaponPartType.Scope:
                    return new Vector3(0, 0.05f, 0);
                case WeaponPartType.Grip:
                    return new Vector3(0, -0.05f, 0.1f);
                case WeaponPartType.Muzzle:
                    return new Vector3(0, 0, 0.5f);
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Updates weapon model transform to match entity transform
        /// </summary>
        private void UpdateModelTransform(Entity weaponEntity, LocalTransform transform)
        {
            if (weaponModels.TryGetValue(weaponEntity, out GameObject weaponRoot))
            {
                if (weaponRoot != null)
                {
                    weaponRoot.transform.position = transform.Position;
                    weaponRoot.transform.rotation = transform.Rotation;
                }
            }
        }

        /// <summary>
        /// Loads prefab by ID from Resources
        /// TODO: Replace with asset management system
        /// </summary>
        private GameObject LoadPrefab(int prefabID)
        {
            // For now, load from Resources folder
            // In production, use addressables or asset bundles
            string path = $"Weapons/Parts/Part_{prefabID}";
            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
                // Try alternative path
                path = $"Prefabs/WeaponParts/Part_{prefabID}";
                prefab = Resources.Load<GameObject>(path);
            }

            return prefab;
        }

        protected override void OnDestroy()
        {
            // Cleanup all spawned models
            foreach (var model in weaponModels.Values)
            {
                if (model != null)
                    GameObject.Destroy(model);
            }

            foreach (var model in partModels.Values)
            {
                if (model != null)
                    GameObject.Destroy(model);
            }

            weaponModels.Clear();
            partModels.Clear();
            prefabCache.Clear();
        }
    }
}
