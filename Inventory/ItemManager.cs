using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
namespace EmergoEntertainment.Inventory
{
    public class ItemManager
    {
        HashSet<IItemInstance> itemInstances;
        static ItemManager instance;

        public ItemManager()
        {
            itemInstances = new HashSet<IItemInstance>();
            instance = this;
        }

        public static IItemInstance CreateItemInstance(Item item)
        {
            if (item.prefab != null &&
                item.prefab.TryGetComponent<IItemBehaviour>(out IItemBehaviour iitem))
            {
                //if a prefab is assigned to the item create an instance by instantiating the prefab.
                GameObject gameObjectInstance = GameObject.Instantiate(item.prefab);
                iitem = gameObjectInstance.GetComponent<IItemBehaviour>();
                return InstantiateGameObjectForItemInstance(iitem, gameObjectInstance, item);
            }
            else
            {
                //else just create an empty game object
                GameObject gameObjectInstance = new GameObject(item.name);
                return InstantiateGameObjectForItemInstance(null, gameObjectInstance, item);
            }
        }
        public static IItemInstance CreateItemInstance(Item item, Transform parent)
        {
            if (item.prefab != null &&
                item.prefab.TryGetComponent<IItemBehaviour>(out IItemBehaviour iitem))
            {
                //if a prefab is assigned to the item create an instance by instantiating the prefab.
                GameObject gameObjectInstance = GameObject.Instantiate(item.prefab, parent);
                iitem = gameObjectInstance.GetComponent<IItemBehaviour>();
                return InstantiateGameObjectForItemInstance(iitem, gameObjectInstance, item);
            }
            else
            {
                //else just create an empty game object
                GameObject gameObjectInstance = new GameObject(item.name);
                gameObjectInstance.transform.SetParent(parent);
                return InstantiateGameObjectForItemInstance(null, gameObjectInstance, item);
            }
        }


        private static IItemInstance InstantiateGameObjectForItemInstance(IItemBehaviour iitem, GameObject gameObjectInstance, Item item)
        {
            gameObjectInstance.transform.position = Vector3.zero;
            gameObjectInstance.SetActive(false);
            IItemInstance itemInstance = null;
            if (iitem != default)
            {
                itemInstance = ItemInstanceFactory(gameObjectInstance, iitem, item);
            }
            else
            {
                DefaultItemInstanceBehaviour behaviour = gameObjectInstance.AddComponent<DefaultItemInstanceBehaviour>();
                ItemInstance<DefaultItemInstanceBehaviour> component = new ItemInstance<DefaultItemInstanceBehaviour>(behaviour, item, gameObjectInstance);
                itemInstance = component;
                behaviour.SetItemInstance(itemInstance);
                behaviour.destroyed += ItemDestroyed;
            }
            instance.itemInstances.Add(itemInstance);
            return itemInstance;
        }

        static IItemInstance ItemInstanceFactory(GameObject gameObject, IItemBehaviour it, Item data)
        {
            //ItemInstance<T> itemInstance = new ItemInstance<T>(controller, item.data, gameObject);
            Type itemInstanceType = it.GetType();
            Type classType = typeof(ItemInstance<>);
            Type[] typeParams = new Type[] { itemInstanceType };
            Type constructedType = classType.MakeGenericType(itemInstanceType);
            IItemInstance itemInstance = Activator.CreateInstance(constructedType, it, data, gameObject) as IItemInstance;
            it.SetItemInstance(itemInstance);
            it.destroyed += ItemDestroyed;
            return itemInstance;            
        }

        private static void ItemDestroyed(IItemBehaviour obj)
        {
            if (instance.itemInstances.Contains(obj.itemInstance))
            {
                obj.itemInstance.Cleanup();
                instance.itemInstances.Remove(obj.itemInstance);
            }
        }

        public static IItemInstance CreateItemInstance(Item item, Vector3 position, bool active = false)
        {
            IItemInstance createdInstance = CreateItemInstance(item);
            createdInstance.gameObject.transform.position = position;
            createdInstance.gameObject.SetActive(true);
            return createdInstance;
        }
    }

}