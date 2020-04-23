using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    public class DefaultItemInstanceBehaviour : MonoBehaviour, IItem
    {
        public Item data => _itemInstance.data;


        public IItemInstance itemInstance => _itemInstance;
        IItemInstance _itemInstance;

        public event Action<IItem> destroyed;

        public bool Drop()
        {
            return true;
        }

        public ItemInstance<T> GetItemInstance<T>() where T : MonoBehaviour, IItem
        {
            if (itemInstance is T)
                return this as ItemInstance<T>;

            return null;
        }

        public void SetItemInstance(IItemInstance itemInstance)
        {
            _itemInstance = itemInstance;
        }

        public bool UseFromInventory()
        {
            return true;
        }

        public void Cleanup()
        {
            Destroy(this.gameObject);
        }
    }


}