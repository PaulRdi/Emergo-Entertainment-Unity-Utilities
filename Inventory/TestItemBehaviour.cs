using System;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    public class TestItemBehaviour : MonoBehaviour, IItemBehaviour
    {
        public Item data => throw new NotImplementedException();

        public IItemInstance itemInstance => _itemInstance;
        IItemInstance _itemInstance;

        public event Action<IItemBehaviour> destroyed;

        public void Cleanup()
        {
        }

        public bool Drop()
        {
            return true;
        }

        public ItemInstance<T> GetItemInstance<T>() where T : MonoBehaviour, IItemBehaviour
        {
            if (itemInstance is ItemInstance<T>)
                return itemInstance as ItemInstance<T>;
            return default;
        }

        public void SetItemInstance(IItemInstance itemInstance)
        {
            _itemInstance = itemInstance;
        }

        public bool UseFromInventory()
        {
            return false;
        }
    }
}