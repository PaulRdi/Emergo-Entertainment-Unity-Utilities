namespace EmergoEntertainment.Inventory
{ 
    public interface IUIToolkitInventoryUI
    {
        /// <summary>
        /// Registers an InventorySlotView to an Inventory and returns the id of the Inventory.slotToItemBatch this view was registered to.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool TryRegisterSlotView(UIToolkitInventorySlot view, int id);
        Inventory inventory { get; }
    }
}