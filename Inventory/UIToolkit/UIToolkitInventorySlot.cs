using EmergoEntertainment.Inventory;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class UIToolkitInventorySlot
{
    public static event Action<UIToolkitInventorySlot, Vector3> OnDragStart;
    public static event Action<UIToolkitInventorySlot> OnDragEnd;
    public static event Action<Vector3> OnDrag;

    public static event Action<UIToolkitInventorySlot> Clicked;
    public event Action<UIToolkitInventorySlot> clicked;

    public event Action<UIToolkitInventorySlot> updated;

    public static UIToolkitInventorySlot hoveringSlot = null;

    private IUIToolkitInventoryUI inventoryManager;
    private Inventory inventory => inventoryManager.inventory;

    private Label stackText;
    private Button button;
    private VisualElement icon;
    private bool dragging = false;

    public int slotID;

    public ItemBatch itemBatch => _itemBatch;
    private ItemBatch _itemBatch => inventoryManager.inventory.slotToItemBatch[slotID];

    public void INIT(Button buttonElement, VisualElement iconElement, IUIToolkitInventoryUI inventoryManager, int slotID)
    {
        this.button = buttonElement;
        this.slotID = slotID;
        this.icon = iconElement;
        this.inventoryManager = inventoryManager;

        buttonElement.RegisterCallback<ClickEvent>(OnClick);
        buttonElement.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        buttonElement.RegisterCallback<PointerUpEvent>(OnPointerUp);
        buttonElement.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        buttonElement.RegisterCallback<PointerOutEvent>(OnPointerOut);
        buttonElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);

        stackText = new Label();
        stackText.name = "StackText";
        stackText.AddToClassList("SlotAmountText");
        stackText.text = "";
        stackText.pickingMode = PickingMode.Ignore;
        stackText.style.display = DisplayStyle.None;
        buttonElement.Add(stackText);
    }

    public void DeInit()
    {
        button.UnregisterCallback<ClickEvent>(OnClick);
        button.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        button.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        button.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
        button.UnregisterCallback<PointerOutEvent>(OnPointerOut);
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
            icon.style.backgroundImage = null;
            stackText.style.display = DisplayStyle.None;
        }
        else
        {
            icon.style.backgroundImage = new StyleBackground(batch.item.Icon);
            stackText.style.display = DisplayStyle.Flex;
            stackText.text = batch.count.ToString();
        }
        updated?.Invoke(this);
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (_itemBatch == null || _itemBatch.item == null)
            return;

        OnDragStart?.Invoke(this, evt.position);
        dragging = true;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (_itemBatch == null || _itemBatch.item == null || !dragging)
            return;

        OnDragEnd?.Invoke(this);
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (_itemBatch == null || _itemBatch.item == null || !dragging)
            return;

        OnDrag?.Invoke(evt.position);
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