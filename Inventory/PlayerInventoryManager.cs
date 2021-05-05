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

        Dictionary<int, InventorySlotView> slotToUI;
        Dictionary<RecipeButton, Recipe> buttonToRecipe;
        bool dirty;

        int currentID;

        public Inventory playerInventory;
        public RectTransform trashObject;

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
            if (this.canvas == null)

            {
                throw new UnityEngine.MissingReferenceException("Player Inventory Manager did not have canvas assigned");
            }
            currentID = 0;
            InventorySlotView.Clicked += InventorySlot_Clicked;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            playerInventory.Updated += PlayerInventory_Updated;
            foreach (Recipe r in recipes)
            {
                InstantiateRecipe(r);
            }
            currentID = 0;

            for (int i= 0; i < playerInventory.slotToItemBatch.Keys.Count; i++)
            {
                InventorySlotView slotView = Instantiate(inventorySlotPrefab, inventorySlotParent);
                slotView.Init(this, canvas);
            }
        }

        private void PlayerInventory_Updated()
        {
            UpdateUI();
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            PhysicsRaycaster raycaster = FindObjectOfType<PhysicsRaycaster>();
            if (raycaster == null)
                Debug.LogError("There was no physics raycaster in the scene you loaded. Inventory Pointer callbacks may not work as intended.");
            else
            {
                eventCamera = raycaster.GetComponent<Camera>();
                
            }
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


        private void InventorySlot_Clicked(InventorySlotView obj)
        {
            if (playerInventory.slotToItemBatch[obj.slotID] == null)
            {
                //had an item
            }
            else
            {
                //did not have an item
            }
        }

        public bool TryRegisterSlotView(InventorySlotView inventorySlot, out int id)
        {
            id = currentID;
            if (playerInventory.slotToItemBatch.ContainsKey(currentID) &&
                !slotToUI.ContainsKey(currentID))
            {
                slotToUI.Add(currentID, inventorySlot);
                currentID++;
                return true;
            }
            return false;
        }


        public void UpdateUI()
        {
            foreach (int slotID in playerInventory.slotToItemBatch.Keys)
            {
                slotToUI[slotID].UpdateItemSlot(playerInventory.slotToItemBatch[slotID]);
            }
        }
    }
}