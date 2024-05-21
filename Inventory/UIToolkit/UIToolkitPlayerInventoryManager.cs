using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace EmergoEntertainment.Inventory
{
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
        [Header("Settings")]
        [SerializeField] bool initOnAwake = false;
        [SerializeField] private int defaultBatchSize;
        [SerializeField] private int defaultSlotCount;

        private Dictionary<int, UIToolkitInventorySlot> indexToInventorySlot;

        private IUIToolkitInventoryVisuals visuals;

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
                    inventory = new Inventory(defaultBatchSize, defaultSlotCount);
                    Debug.LogWarning("Did not provide an inventory on init. Initializing new inventory.");
                }
                visuals = GetComponent<IUIToolkitInventoryVisuals>();
                if (visuals == null)
                {
                    Debug.LogError("No visuals builder found on Inventory Manager");
                    return;
                }
                InitInventory(inventory);
                visuals.INIT(this);
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }

            visuals.INIT(this);
            UpdateUI();
        }

        void InitInventory(Inventory inventory)
        {
            _instance = this;
            playerInventory = inventory;

            //Architecture of the Visual elements is vaguely defined here 
            visuals.CreateInventoryHolderVisuals();
            for (int i = 0; i < inventory.slotToItemBatch.Count; i++)
            {
                var slotVisuals = visuals.CreateItemSlotVisuals(i);
                var newInventorySlot = new UIToolkitInventorySlot();
                newInventorySlot.INIT(slotVisuals, this, i);
                TryRegisterSlotView(newInventorySlot, i);
            }
            playerInventory.Updated += UpdateUI;
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