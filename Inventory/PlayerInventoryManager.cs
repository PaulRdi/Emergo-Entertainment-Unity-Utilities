﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    public class PlayerInventoryManager : MonoBehaviour
    {
        public static PlayerInventoryManager instance
        {
            get
            {
                _instance.dirty = true;
                return _instance;
            }
        }
        static PlayerInventoryManager _instance;

        [SerializeField] Recipe[] recipes;
        [SerializeField] GameObject recipeEntryPrefab;
        [SerializeField] Transform recipeListParent;
        [SerializeField] InventorySlotView inventorySlotPrefab;
        [SerializeField] Transform inventorySlotParent;
        [SerializeField] int inventorySize;
        [SerializeField] int maxBatchSize = 25;
        [SerializeField] bool useRecipes = false;
        [SerializeField] bool initOnAwake = false;

        Dictionary<int, InventorySlotView> slotToUI;
        Dictionary<RecipeButton, Recipe> buttonToRecipe;
        bool dirty;

        int currentID;

        public Inventory playerInventory;
        public GameObject trashObject;

        public void Awake()
        {
            if (initOnAwake)
                INIT();
        }

        public void INIT()
        {
            if (_instance == null)
            {
                InitInventory();
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

        void InitInventory()
        {
            _instance = this;
            buttonToRecipe = new Dictionary<RecipeButton, Recipe>();
            slotToUI = new Dictionary<int, InventorySlotView>();
            currentID = 0;
            InventorySlotView.Clicked += InventorySlot_Clicked;
            playerInventory = new Inventory(maxBatchSize, inventorySize);


            foreach (Recipe r in recipes)
            {
                InstantiateRecipe(r);
            }
            currentID = 0;

            for (int i= 0; i < inventorySize; i++)
            {
                InventorySlotView slotView = Instantiate(inventorySlotPrefab, inventorySlotParent);
                slotView.Init();
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

        internal bool TryRegisterSlotView(InventorySlotView inventorySlot, out int id)
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