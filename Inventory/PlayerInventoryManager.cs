using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace EmergoEntertainment.Inventory
{
    public class PlayerInventoryManager : MonoBehaviour, IInventoryUI
    {
        public static PlayerInventoryManager instance
        {
            get
            {
                if (_instance != null)
                {
                    _instance.dirty = true;
                    return _instance;
                }
                return default;
            }
        }
        static PlayerInventoryManager _instance;

        [SerializeField] Recipe[] recipes;
        [SerializeField] GameObject recipeEntryPrefab;
        [SerializeField] Transform recipeListParent;
        [SerializeField] InventorySlotView inventorySlotPrefab;
        [SerializeField] Transform inventorySlotParent;
        [SerializeField] bool useRecipes = false;
        [SerializeField] bool initOnAwake = false;
        [SerializeField] Canvas canvas;
        public Camera eventCamera { get; private set; }

        public Dictionary<int, InventorySlotView> slotToUI;
        Dictionary<RecipeButton, Recipe> buttonToRecipe;
        
        bool dirty;

        public Inventory inventory => playerInventory;
        public Inventory playerInventory;

        //Not used withing the package
        public static event Action inventorySetupFinished;

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
                    inventory = new Inventory(1, 1);
                    Debug.LogWarning("Did not provide an inventory on init. Initializing new inventory.");
                }

                InitInventory(inventory);
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void LateUpdate()
        {
            if (dirty)
            {
                dirty = false;
                UpdateUI();
            }
        }

        void InitInventory(Inventory inventory)
        {
            _instance = this;
            playerInventory = inventory;
            buttonToRecipe = new Dictionary<RecipeButton, Recipe>();
            slotToUI = new Dictionary<int, InventorySlotView>();
            TrySetEventCamera();

            if (this.canvas == null)

            {
                throw new UnityEngine.MissingReferenceException("Player Inventory Manager did not have canvas assigned");
            }
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            playerInventory.Updated += PlayerInventory_Updated;
            foreach (Recipe r in recipes)
            {
                InstantiateRecipe(r);
            }


            List<InventorySlotView> existingInventorySlots = GetComponentsInChildren<InventorySlotView>(true).ToList();

            for (int i = 0; i < playerInventory.slotToItemBatch.Keys.Count; i++)
            {

                InventorySlotView slotView = existingInventorySlots.FirstOrDefault(invSlot => invSlot.slotID == i);
                if (slotView == default)
                {
                    slotView = Instantiate(inventorySlotPrefab, inventorySlotParent);
                }
                else
                {
                    existingInventorySlots.Remove(slotView);
                }
                slotView.Init(this, canvas, i);
            }
            foreach (InventorySlotView nonRegisteredSlot in existingInventorySlots)
            {
                nonRegisteredSlot.gameObject.SetActive(false);
                //nonRegisteredSlot.gameObject.name = "DISABLED " + nonRegisteredSlot.name;
            }

            inventorySetupFinished.Invoke();
        }

        private void PlayerInventory_Updated()
        {
            UpdateUI();
        }

        private void TrySetEventCamera()
        {
            PhysicsRaycaster raycaster3D = FindObjectOfType<PhysicsRaycaster>();
            Physics2DRaycaster raycaster2D = FindObjectOfType<Physics2DRaycaster>();

            if (raycaster3D == null && raycaster2D == null)
                Debug.LogError("There was no physics raycaster in the scene you loaded. Inventory Pointer callbacks may not work as intended.");
            else
            {
                if (raycaster2D != null)
                    eventCamera = raycaster2D.GetComponent<Camera>();
                else
                    eventCamera = raycaster3D.GetComponent<Camera>();
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            TrySetEventCamera();
            foreach (InventorySlotView slotView in slotToUI.Values)
            {
                slotView.SetEventCamera(eventCamera);
            }
        }

        private void InstantiateRecipe(Recipe r)
        {
            GameObject inst = Instantiate(recipeEntryPrefab, recipeListParent);
            inst.GetComponentInChildren<TextMeshProUGUI>().text = r.name;
            RecipeButton btn = inst.GetComponent<RecipeButton>();
            btn.clicked += RecipeClicked;
            buttonToRecipe.Add(btn, r);
        }
        private void RecipeClicked(RecipeButton btn)
        {
            if (!buttonToRecipe.ContainsKey(btn))
                throw new System.Exception("tried to click a recipe which was not registered in buttonToRecipe. This should not happen...");

            Recipe toCraft = buttonToRecipe[btn];
            bool craftingAllowed = true;
            foreach (RecipeComponent c in toCraft.Components)
            {
                if (playerInventory.GetTotalItemAmount(c.Item) < c.Amount)
                {
                    craftingAllowed = false;
                    break;
                }
            }

            if (craftingAllowed)
            {
                Debug.Log("could craft");
                playerInventory.CraftItem(toCraft);
            }
            else
            {
                Debug.Log("could not craft");
            }
        }

        public bool TryRegisterSlotView(InventorySlotView inventorySlot, int id)
        {
            if (playerInventory.slotToItemBatch.ContainsKey(id) &&
                !slotToUI.ContainsKey(id))
            {
                slotToUI.Add(id, inventorySlot);
                return true;
            }
            return false;
        }

        /// <summary>
        /// CAUTION: Dosent work when the inventory is reduced and increased twice
        /// </summary>
        public void UpdateUI()
        {
            List<InventorySlotView> existingInventorySlots = GetComponentsInChildren<InventorySlotView>(true).ToList();
            foreach (int slotID in playerInventory.slotToItemBatch.Keys)
            {
                if (slotToUI.ContainsKey(slotID))
                    slotToUI[slotID].UpdateItemSlot(playerInventory.slotToItemBatch[slotID]);
                else
                {
                    InventorySlotView slotView = existingInventorySlots.FirstOrDefault(invSlot => invSlot.slotID == slotID);
                    if (slotView == default)
                    {
                        slotView = Instantiate(inventorySlotPrefab, inventorySlotParent);
                    }
                    else
                    {
                        existingInventorySlots.Remove(slotView);
                    }
                    slotView.Init(this, canvas, slotID);
                    slotView.gameObject.SetActive(true);
                }
            }


            // Find and deactivate all slot prefabs without an active slot id
            List<InventorySlotView> activeInventorySlotsviews = GetComponentsInChildren<InventorySlotView>(false).ToList();
            slotToUI = slotToUI.Where(slot => playerInventory.slotToItemBatch.ContainsKey(slot.Key)).ToDictionary(slot => slot.Key, slot => slot.Value);

            foreach (var slotView in activeInventorySlotsviews)
            {
                if (slotToUI.ContainsValue(slotView))
                {
                    slotView.gameObject.SetActive(true);
                    //slotView.transform.SetAsFirstSibling();
                }
                else
                    slotView.gameObject.SetActive(false);

            }
        }
    }
}