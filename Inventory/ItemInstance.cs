using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace EmergoEntertainment.Inventory
{
    public class ItemInstance<T> : IItemInstance where T : MonoBehaviour, IItemBehaviour
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

        public J GetController<J>() where J : MonoBehaviour, IItemBehaviour
        {
            if (component is J)
                return component as J;
            return default;
        }

        public bool TryGetController<J> (out J controller) where J : MonoBehaviour, IItemBehaviour
        {
            controller = GetController<J>();
            if (controller == default)
                return false;
            return true;
        }

        public void Cleanup()
        {
            component.Cleanup();
        }

        public bool Drop()
        {
            return component.Drop();
        }

        public T1 GetData<T1>() where T1 : Item
        {
            if (this.data.GetType() == typeof(T1))
                return this.data as T1;
            return default;
        }

        public bool TryGetData<T1>(out T1 data) where T1 : Item
        {
            data = default;
            if (this.data != default &&
                this.data.GetType() == typeof(T1))
            {
                data = this.data as T1;
                return true;
            }
            return false;
        }
    }

}