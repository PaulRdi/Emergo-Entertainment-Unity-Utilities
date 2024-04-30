using EmergoEntertainment.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitInventorySlot
{
    public static event Action<UIToolkitInventorySlot> OnDragStart;
    public static event Action<UIToolkitInventorySlot> OnDragEnd;

    public static event Action<UIToolkitInventorySlot> Clicked;
    public event Action<UIToolkitInventorySlot> clicked;

    public event Action<UIToolkitInventorySlot> updated;

    public static UIToolkitInventorySlot hoveringSlot = null;

    private IUIToolkitInventoryUI inventoryManager;
    private Inventory inventory => inventoryManager.inventory;

    private Label stackText;
    private Button button;

    public int slotID;

    public ItemBatch itemBatch => _itemBatch;
    private ItemBatch _itemBatch => inventoryManager.inventory.slotToItemBatch[slotID];

    public void INIT(Button button, IUIToolkitInventoryUI inventoryManager, int slotID)
    {
        this.button = button;
        this.slotID = slotID;
        this.inventoryManager = inventoryManager;

        button.RegisterCallback<ClickEvent>(OnClick);
        button.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        button.RegisterCallback<PointerUpEvent>(OnPointerUp);
        button.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        button.RegisterCallback<PointerOutEvent>(OnPointerOut);

        stackText = new Label();
        stackText.AddToClassList("SlotAmountText");
        stackText.text = "1";
        stackText.pickingMode = PickingMode.Ignore;
        stackText.style.display = DisplayStyle.None;
        button.Add(stackText);

        inventoryManager.TryRegisterSlotView(this, slotID);
    }

    private void OnClick(ClickEvent click)
    {
        clicked?.Invoke(this);
        Clicked?.Invoke(this);
    }

    public void UpdateItemSlot(ItemBatch batch)
    {
        if (batch == null || batch.item == null)
        {
            button.style.backgroundImage = null;
            stackText.style.display = DisplayStyle.None;
        }
        else
        {
            button.style.backgroundImage = new StyleBackground(batch.item.Icon);
            stackText.style.display = DisplayStyle.Flex;
            stackText.text = batch.count.ToString();
            Debug.Log(stackText.text);
        }
        updated?.Invoke(this);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (_itemBatch == null || _itemBatch.item == null)
            return;

        OnDragStart?.Invoke(this);
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (_itemBatch == null || _itemBatch.item == null)
            return;

        OnDragEnd?.Invoke(this);
    }

    //Assuming slots don't overlap
    private void OnPointerEnter(PointerEnterEvent evt) 
    {
        hoveringSlot = this;
    }

    //Assuming slots don't overlap
    private void OnPointerOut(PointerOutEvent evt) 
    {
        hoveringSlot = null;
    }

    #region Inventory Slot Dropped
    public bool TryMoveItemFromOtherInventorySlotToThis(UIToolkitInventorySlot inventorySlot)
    {
        bool success = false;
        //Take out items first
        if (inventory.TryTakeItemFromSlot(inventorySlot.slotID, out List<IItemInstance> items))
        {
            //Check if we can add the items to the own slot
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var iitemInstance = items[i];
                if (inventory.TryAddItemInstanceToSlot(slotID, iitemInstance))
                {
                    success = true;
                    items.RemoveAt(i);
                }
            }

            //If we have items left, put them back in the original slot
            foreach (var itemInstance in items)
            {
                inventory.TryAddItemInstanceToSlot(inventorySlot.slotID, itemInstance);
            }
        }
        if (success)
            inventory.UpdateEmptyBatches();

        return success;
    }
    #endregion
}