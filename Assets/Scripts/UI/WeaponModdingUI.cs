using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using ZoneSurvival.Weapons;
using ZoneSurvival.Items;

namespace ZoneSurvival.UI
{
    /// <summary>
    /// Weapon modding UI - Tarkov-style weapon workbench
    /// Allows drag-and-drop part attachment/detachment
    ///
    /// Usage:
    /// 1. Attach this MonoBehaviour to UI GameObject with UIDocument
    /// 2. Set up UXML with required elements (see documentation)
    /// 3. Call OpenForWeapon(weaponEntity) to open UI for specific weapon
    ///
    /// UI Elements (UXML):
    /// - weapon-view: Visual display of weapon
    /// - part-slots-container: List of available part slots
    /// - available-parts-container: List of parts in inventory
    /// - part-stats-panel: Shows stats of selected part
    /// - attach-button: Attach selected part
    /// - detach-button: Detach selected part
    /// - close-button: Close modding UI
    /// </summary>
    public class WeaponModdingUI : MonoBehaviour
    {
        [Header("UI Document")]
        public UIDocument uiDocument;

        [Header("Prefabs")]
        public VisualTreeAsset partSlotTemplate;
        public VisualTreeAsset partItemTemplate;

        // UI Elements
        private VisualElement root;
        private VisualElement weaponView;
        private VisualElement partSlotsContainer;
        private VisualElement availablePartsContainer;
        private VisualElement partStatsPanel;
        private Button attachButton;
        private Button detachButton;
        private Button closeButton;
        private Label weaponNameLabel;
        private Label weaponStatsLabel;

        // State
        private Entity currentWeapon = Entity.Null;
        private Entity selectedPart = Entity.Null;
        private WeaponPartType selectedSlot;
        private EntityManager entityManager;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            SetupUI();
        }

        private void SetupUI()
        {
            root = uiDocument.rootVisualElement;

            // Get UI elements
            weaponView = root.Q<VisualElement>("weapon-view");
            partSlotsContainer = root.Q<VisualElement>("part-slots-container");
            availablePartsContainer = root.Q<VisualElement>("available-parts-container");
            partStatsPanel = root.Q<VisualElement>("part-stats-panel");
            attachButton = root.Q<Button>("attach-button");
            detachButton = root.Q<Button>("detach-button");
            closeButton = root.Q<Button>("close-button");
            weaponNameLabel = root.Q<Label>("weapon-name");
            weaponStatsLabel = root.Q<Label>("weapon-stats");

            // Setup button callbacks
            attachButton?.RegisterCallback<ClickEvent>(OnAttachButtonClicked);
            detachButton?.RegisterCallback<ClickEvent>(OnDetachButtonClicked);
            closeButton?.RegisterCallback<ClickEvent>(OnCloseButtonClicked);

            // Hide UI by default
            root.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Opens modding UI for specified weapon
        /// </summary>
        public void OpenForWeapon(Entity weaponEntity)
        {
            if (!entityManager.Exists(weaponEntity))
                return;

            if (!entityManager.HasComponent<WeaponStateData>(weaponEntity))
                return;

            currentWeapon = weaponEntity;
            selectedPart = Entity.Null;

            // Show UI
            root.style.display = DisplayStyle.Flex;

            // Refresh UI
            RefreshWeaponInfo();
            RefreshPartSlots();
            RefreshAvailableParts();
            RefreshPartStats();
        }

        /// <summary>
        /// Closes modding UI
        /// </summary>
        public void Close()
        {
            root.style.display = DisplayStyle.None;
            currentWeapon = Entity.Null;
            selectedPart = Entity.Null;
        }

        /// <summary>
        /// Refreshes weapon name and stats display
        /// </summary>
        private void RefreshWeaponInfo()
        {
            if (currentWeapon == Entity.Null)
                return;

            // Get weapon data
            var itemData = entityManager.GetComponentData<ItemData>(currentWeapon);
            var weaponState = entityManager.GetComponentData<WeaponStateData>(currentWeapon);

            // Update name
            weaponNameLabel.text = itemData.ItemName.ToString();

            // Update stats
            string stats = $"Accuracy: {weaponState.CalculatedAccuracy:P0}\n" +
                          $"Recoil: {weaponState.CalculatedRecoil:F2}\n" +
                          $"Damage: {weaponState.CalculatedDamage:F0}\n" +
                          $"Range: {weaponState.CalculatedRange:F0}m\n" +
                          $"Jam Chance: {weaponState.CalculatedJamChance:P1}\n" +
                          $"Ergonomics: {weaponState.CalculatedErgo:F2}s";
            weaponStatsLabel.text = stats;
        }

        /// <summary>
        /// Refreshes list of part slots on weapon
        /// </summary>
        private void RefreshPartSlots()
        {
            partSlotsContainer.Clear();

            if (currentWeapon == Entity.Null)
                return;

            // Get part slots
            var slotsBuffer = entityManager.GetBuffer<WeaponPartSlotDefinition>(currentWeapon);
            var partsBuffer = entityManager.GetBuffer<WeaponPartElement>(currentWeapon);

            // Create UI element for each slot
            foreach (var slot in slotsBuffer)
            {
                VisualElement slotElement = CreatePartSlotElement(slot, partsBuffer);
                partSlotsContainer.Add(slotElement);
            }
        }

        /// <summary>
        /// Creates UI element for a part slot
        /// </summary>
        private VisualElement CreatePartSlotElement(WeaponPartSlotDefinition slot,
            DynamicBuffer<WeaponPartElement> partsBuffer)
        {
            VisualElement slotElement = partSlotTemplate?.Instantiate() ?? new VisualElement();

            // Set slot type label
            var slotLabel = slotElement.Q<Label>("slot-name");
            if (slotLabel != null)
                slotLabel.text = slot.SlotType.ToString();

            // Find attached part (if any)
            Entity attachedPart = Entity.Null;
            for (int i = 0; i < partsBuffer.Length; i++)
            {
                if (partsBuffer[i].SlotType == slot.SlotType)
                {
                    attachedPart = partsBuffer[i].PartEntity;
                    break;
                }
            }

            // Show attached part or "Empty"
            var partNameLabel = slotElement.Q<Label>("part-name");
            if (attachedPart != Entity.Null && entityManager.Exists(attachedPart))
            {
                var partData = entityManager.GetComponentData<WeaponPartData>(attachedPart);
                if (partNameLabel != null)
                    partNameLabel.text = partData.PartName.ToString();

                // Click to select for detachment
                slotElement.RegisterCallback<ClickEvent>(evt =>
                {
                    selectedPart = attachedPart;
                    selectedSlot = slot.SlotType;
                    RefreshPartStats();
                });
            }
            else
            {
                if (partNameLabel != null)
                    partNameLabel.text = slot.IsRequired ? "[REQUIRED]" : "[Empty]";

                // Click to select slot for attachment
                slotElement.RegisterCallback<ClickEvent>(evt =>
                {
                    selectedPart = Entity.Null;
                    selectedSlot = slot.SlotType;
                    RefreshPartStats();
                });
            }

            return slotElement;
        }

        /// <summary>
        /// Refreshes list of available parts in inventory
        /// TODO: Integrate with inventory system to get actual parts
        /// </summary>
        private void RefreshAvailableParts()
        {
            availablePartsContainer.Clear();

            // TODO: Query inventory for available weapon parts
            // For now, query all WeaponPartData entities in world
            var query = entityManager.CreateEntityQuery(typeof(WeaponPartData));
            var parts = query.ToEntityArray(Allocator.Temp);

            foreach (var partEntity in parts)
            {
                VisualElement partElement = CreatePartItemElement(partEntity);
                availablePartsContainer.Add(partElement);
            }

            parts.Dispose();
        }

        /// <summary>
        /// Creates UI element for an available part
        /// </summary>
        private VisualElement CreatePartItemElement(Entity partEntity)
        {
            var partData = entityManager.GetComponentData<WeaponPartData>(partEntity);

            VisualElement partElement = partItemTemplate?.Instantiate() ?? new VisualElement();

            // Set part name
            var nameLabel = partElement.Q<Label>("part-name");
            if (nameLabel != null)
                nameLabel.text = partData.PartName.ToString();

            // Set part type
            var typeLabel = partElement.Q<Label>("part-type");
            if (typeLabel != null)
                typeLabel.text = partData.PartType.ToString();

            // Set condition bar
            var conditionBar = partElement.Q<VisualElement>("condition-bar");
            if (conditionBar != null)
                conditionBar.style.width = Length.Percent(partData.Condition * 100f);

            // Click to select
            partElement.RegisterCallback<ClickEvent>(evt =>
            {
                selectedPart = partEntity;
                RefreshPartStats();
            });

            return partElement;
        }

        /// <summary>
        /// Refreshes part stats panel
        /// </summary>
        private void RefreshPartStats()
        {
            if (selectedPart == Entity.Null || !entityManager.Exists(selectedPart))
            {
                partStatsPanel.style.display = DisplayStyle.None;
                return;
            }

            partStatsPanel.style.display = DisplayStyle.Flex;

            var partData = entityManager.GetComponentData<WeaponPartData>(selectedPart);

            // Update stats display
            var statsLabel = partStatsPanel.Q<Label>("stats-text");
            if (statsLabel != null)
            {
                string stats = $"{partData.PartName}\n" +
                              $"Type: {partData.PartType}\n" +
                              $"Condition: {partData.Condition:P0}\n" +
                              $"Weight: {partData.Weight:F2}kg\n" +
                              $"\nModifiers:\n" +
                              $"Accuracy: {partData.AccuracyModifier:+0.00;-0.00}\n" +
                              $"Recoil: {partData.RecoilModifier:+0.00;-0.00}\n" +
                              $"Range: {partData.RangeModifier:+0;-0}m\n" +
                              $"Ergo: {partData.ErgoModifier:+0.00;-0.00}s\n" +
                              $"Damage: {partData.DamageModifier:+0.0%;-0.0%}";
                statsLabel.text = stats;
            }
        }

        /// <summary>
        /// Attach button clicked
        /// </summary>
        private void OnAttachButtonClicked(ClickEvent evt)
        {
            if (currentWeapon == Entity.Null || selectedPart == Entity.Null)
                return;

            // Add attach request to weapon entity
            entityManager.AddComponentData(currentWeapon, new PartAttachRequest
            {
                PartEntity = selectedPart,
                TargetSlot = selectedSlot
            });

            // Refresh UI next frame (after system processes request)
            Invoke(nameof(RefreshAllUI), 0.1f);
        }

        /// <summary>
        /// Detach button clicked
        /// </summary>
        private void OnDetachButtonClicked(ClickEvent evt)
        {
            if (currentWeapon == Entity.Null || selectedPart == Entity.Null)
                return;

            // Add detach request to weapon entity
            entityManager.AddComponentData(currentWeapon, new PartDetachRequest
            {
                PartEntity = selectedPart
            });

            // Refresh UI next frame
            Invoke(nameof(RefreshAllUI), 0.1f);
        }

        /// <summary>
        /// Close button clicked
        /// </summary>
        private void OnCloseButtonClicked(ClickEvent evt)
        {
            Close();
        }

        /// <summary>
        /// Refreshes entire UI
        /// </summary>
        private void RefreshAllUI()
        {
            RefreshWeaponInfo();
            RefreshPartSlots();
            RefreshAvailableParts();
            RefreshPartStats();
        }
    }
}
