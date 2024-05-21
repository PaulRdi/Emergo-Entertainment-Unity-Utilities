using EmergoEntertainment.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public interface IUIToolkitInventoryVisuals
{
    public abstract Button CreateItemSlotVisuals(int index, out UIToolkitInventorySlot slot);
    public abstract void CreateInventoryHolderVisuals();
    public abstract void INIT(IUIToolkitInventoryUI inventoryManager);
}