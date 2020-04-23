using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace EmergoEntertainment.Inventory
{
    public class ItemInstance<T> : IItemInstance where T : MonoBehaviour, IItem
    {
        public GameObject gameObject => _gameObject;
        GameObject _gameObject;

        public T component => _component;
        T _component;



        public Item data => _data;
        Item _data;

        public bool UseFromInventory()
        {
            return component.UseFromInventory();
        }

        public ItemInstance(T component, Item data, GameObject gameObject)
        {
            _gameObject = gameObject;
            _component = component;
            _data = data;
        }

        public J GetController<J>() where J : MonoBehaviour, IItem
        {
            if (component is J)
                return component as J;
            return null;
        }

        public void Cleanup()
        {
            component.Cleanup();
        }

        public bool Drop()
        {
            return component.Drop();
        }
    }

}