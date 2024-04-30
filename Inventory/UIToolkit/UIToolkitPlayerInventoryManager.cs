using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace EmergoEntertainment.Inventory
{
    /// <summary>
    /// Uses pre-defined stylesheet names to create the visual elements for the inventory UI
    /// "InventoryHolder" is the parent element for all inventory slots
    /// "InventorySlot" is the element (button) for each individual slot
    /// "SlotAmountText" is the element (label) for the stack size text
    /// </summary>
    public class UIToolkitPlayerInventoryManager : MonoBehaviour, IUIToolkitInventoryUI
    {
        public static UIToolkitPlayerInventoryManager instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                return default;
            }
        }
        static UIToolkitPlayerInventoryManager _instance;

        public Inventory inventory => playerInventory;
        public Inventory playerInventory;
        [SerializeField] bool initOnAwake = false;

        private Dictionary<int, UIToolkitInventorySlot> indexToInventorySlot;

        [Header("Inventory Settings")]
        [SerializeField] private float cellSize = 100;
        [SerializeField] private float spacing = 10;
        [SerializeField] private int slotsPerRow = 3;

        private VisualElement root;
        private VisualElement inventoryHolder;

        public bool TryRegisterSlotView(UIToolkitInventorySlot slot, int id)
        {
            if (indexToInventorySlot == null)
                indexToInventorySlot = new Dictionary<int, UIToolkitInventorySlot>();

            if (indexToInventorySlot.ContainsKey(id))
            {
                Debug.LogError("Inventory already has a slot with ID: " + id);
                return false;
            }

            indexToInventorySlot[id] = slot;
            return true;
        }

        public void Awake()
        {
            if (initOnAwake)
                INIT();
        }

        public void INIT(Inventory inventory = default)
        {
            if (_instance == null)
            {
                if (inventory == default)
                {
                    inventory = new Inventory(3, 8);
                    Debug.LogWarning("Did not provide an inventory on init. Initializing new inventory.");
                }

                InitInventory(inventory);
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            UpdateUI();
        }

        void InitInventory(Inventory inventory)
        {
            _instance = this;
            playerInventory = inventory;

            root = GetComponent<UIDocument>().rootVisualElement;

            inventoryHolder = new VisualElement();
            inventoryHolder.name = "Inventory";
            inventoryHolder.AddToClassList("Inventory");
            inventoryHolder.style.width = slotsPerRow * (cellSize + (2 * spacing));
            inventoryHolder.style.height = Mathf.Ceil((float)inventory.numSlots / (float)slotsPerRow) * (cellSize + (2 * spacing));
            inventoryHolder.pickingMode = PickingMode.Ignore;

            root.Add(inventoryHolder);

            int slotCount = inventory.slotToItemBatch.Count;
            for (int i = 0; i < slotCount; i++)
            {
                var slot = CreateNewEmptyItemSlot(i);
                inventoryHolder.Add(slot);
            }

            inventoryHolder.style.display = DisplayStyle.None;
            playerInventory.Updated += UpdateUI;
        }

        public Button CreateNewEmptyItemSlot(int index)
        {
            var button = new Button();
            button.AddToClassList("InventorySlot");
            button.name = "InventorySlot_" + index;

            button.style.width = cellSize;
            button.style.height = cellSize;
            button.style.marginLeft = spacing;
            button.style.marginRight = spacing;
            button.style.marginTop = spacing;
            button.style.marginBottom = spacing;


            var slot = new UIToolkitInventorySlot();
            slot.INIT(button, this, index);

            return button;
        }

        private void UpdateUI () 
        { 
            var itemSlots = indexToInventorySlot.Values;

            foreach (int slotID in playerInventory.slotToItemBatch.Keys)
            {
                if (indexToInventorySlot.ContainsKey(slotID))
                    indexToInventorySlot[slotID].UpdateItemSlot(playerInventory.slotToItemBatch[slotID]);
                else
                {
                    //Add code that adjusts inventory if inventory size was changed at runtime?
                }
            }
        }
    }
}