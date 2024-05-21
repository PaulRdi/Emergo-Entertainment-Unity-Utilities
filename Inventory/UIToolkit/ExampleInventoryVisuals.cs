using EmergoEntertainment.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ExampleInventoryVisuals : MonoBehaviour, IUIToolkitInventoryVisuals
{
    [Header("Inventory Settings")]
    [SerializeField] private float cellSize = 100;
    [SerializeField] private float spacing = 10;
    [SerializeField] private int slotsPerRow = 3;

    private IUIToolkitInventoryUI inventoryManager;
    private Inventory inventory => inventoryManager.inventory;
    private VisualElement root;
    private VisualElement inventoryHolder;

    public Button CreateItemSlotVisuals(int index, out UIToolkitInventorySlot slot)
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

        var icon = new VisualElement();
        icon.name = "Icon";
        icon.style.width = cellSize * 0.8f;
        icon.style.height = cellSize * 0.8f;
        button.Add(icon);

        inventoryHolder.Add(button);
        slot = new UIToolkitInventorySlot();
        slot.INIT(button, icon, inventoryManager, index);

        return button;
    }

    public void CreateInventoryHolderVisuals()
    {
        inventoryHolder = new VisualElement();
        inventoryHolder.name = "Inventory";
        inventoryHolder.AddToClassList("Inventory");
        inventoryHolder.style.width = slotsPerRow * (cellSize + (2 * spacing));
        inventoryHolder.style.height = Mathf.Ceil((float)inventory.numSlots / (float)slotsPerRow) * (cellSize + (2 * spacing));
        inventoryHolder.pickingMode = PickingMode.Ignore;

        root.Add(inventoryHolder);
    }

    public void INIT(IUIToolkitInventoryUI inventoryManager)
    {
        this.inventoryManager = inventoryManager;
        root = GetComponent<UIDocument>().rootVisualElement;
    }
}
