using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
                item.prefab.TryGetComponent<IItem>(out IItem iitem))
            {
                //if a prefab is assigned to the item create an instance by instantiating the prefab.
                GameObject gameObjectInstance = GameObject.Instantiate(item.prefab);
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
                item.prefab.TryGetComponent<IItem>(out IItem iitem))
            {
                //if a prefab is assigned to the item create an instance by instantiating the prefab.
                GameObject gameObjectInstance = GameObject.Instantiate(item.prefab, parent);
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


        private static IItemInstance InstantiateGameObjectForItemInstance(IItem iitem, GameObject gameObjectInstance, Item item)
        {
            gameObjectInstance.transform.position = Vector3.zero;
            gameObjectInstance.SetActive(false);
            IItemInstance itemInstance = null;
            if (iitem != null)
            {
                itemInstance = ItemInstanceFactory(gameObjectInstance, iitem);
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

        static IItemInstance ItemInstanceFactory(GameObject gameObject, IItem item)
        {
            IItemInstance instance = null;
            //switchcase.
            return instance;

            IItemInstance Churn<T>(GameObject go, IItem it) where T : MonoBehaviour, IItem
            {
                T controller = gameObject.GetComponent<T>();
                if (controller == null)
                    throw new NullReferenceException("prefab didnt have correct controller component.");

                ItemInstance<T> itemInstance = new ItemInstance<T>(controller, item.data, gameObject);
                it.SetItemInstance(itemInstance);
                it.destroyed += ItemDestroyed;
                return itemInstance;
            }
        }

        private static void ItemDestroyed(IItem obj)
        {
            if (instance.itemInstances.Contains(obj.itemInstance))
            {
                obj.itemInstance.Cleanup();
                instance.itemInstances.Remove(obj.itemInstance);
            }
        }

        public static void RegisterItemInstance<T>(T controller, Item data) where T : MonoBehaviour, IItem
        {
            instance.itemInstances.Add(ItemInstanceFactory(controller.gameObject, controller));
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