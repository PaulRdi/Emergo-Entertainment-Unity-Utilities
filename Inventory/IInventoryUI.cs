using System;
namespace EmergoEntertainment.Inventory
{
    public interface IInventoryUI
    {
        /// <summary>
        /// Registers an InventorySlotView to an Inventory and returns the id of the Inventory.slotToItemBatch this view was registered to.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        bool TryRegisterSlotView(InventorySlotView view, int id);
        UnityEngine.Camera eventCamera { get; }
        Inventory inventory { get; }
    }
}
