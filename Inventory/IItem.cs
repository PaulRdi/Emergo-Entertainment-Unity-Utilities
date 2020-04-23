using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace EmergoEntertainment.Inventory
{
    public interface IItem
    {
        ItemInstance<T> GetItemInstance<T>() where T : MonoBehaviour, IItem;
        Item data { get; }
        IItemInstance itemInstance { get; }
        void SetItemInstance(IItemInstance itemInstance);
        event Action<IItem> destroyed;
        bool UseFromInventory();
        bool Drop();
        void Cleanup();
    }
}