using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergoEntertainment.Inventory
{
    public interface IInventorySlotInteractable
    {
        void InventorySlotDroppedOnObject(InventorySlotView inventorySlot);
    }
}
