using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EmergoEntertainment.Inventory
{
    public class ItemBatch 
    {
        public Item item {
            get
            {
                if (items.Count > 0)
                    return items[0].data;
                return null;
            }
        }
        public readonly List<IItemInstance> items;


        public int count
        {
            get => items.Count;
        }
        public float fillLevel
        {
            get
            {
                return count * item.stackWeight;
            }
        }
        private bool CanAddItemInstance(IItemInstance itemInstance)
        {
            if (item != itemInstance.data)
                return false;
            if (items.Any(i => i == itemInstance))
                return false;

            return true;            
        }
        public List<IItemInstance> AddNew(int amount)
        {
            List<IItemInstance> addedInstances = new List<IItemInstance>();
            for (int i = 0; i < amount; i++)
            {                
                IItemInstance itemInstance = ItemManager.CreateItemInstance(item);
                if (!TryAdd(itemInstance))
                {
                    throw new System.Exception("Could not add item to inventory batch \n" + itemInstance?.ToString());
                }
                addedInstances.Add(itemInstance);
            }

            return addedInstances;
        }
        public bool TryAdd(IItemInstance itemInstance)
        {
            if (CanAddItemInstance(itemInstance))
            {
                items.Add(itemInstance);
                return true;
            }
            return false;
        }
        public List<IItemInstance> Take(int amount)
        {
            int realAmount = Mathf.Clamp(amount, 0, count);
            List<IItemInstance> output = items.GetRange(0, realAmount);
            items.RemoveRange(0, realAmount);
            return output;
        }
        /// <summary>
        /// Returns the items which were consumed.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public List<IItemInstance> UseMultiple(int amount)
        {
            int realAmount = Mathf.Clamp(amount, 0, count);
            List<IItemInstance> selectedItems = items.GetRange(0, realAmount);
            List<IItemInstance> toRemove = new List<IItemInstance>();
            selectedItems.ForEach(inst => UseSingle(inst, ref toRemove));
            items.RemoveAll(i => toRemove.Contains(i));
            return selectedItems;
        }

        private void UseSingle(IItemInstance inst, ref List<IItemInstance> toRemove)
        {
            if (inst.data.consumeOnUse)
                toRemove.Add(inst);
        }
        public bool TryUseSingle(IItemInstance inst)
        {
            if (!items.Contains(inst))
                return false;
            items.Remove(inst);
            return true;
        }
        public ItemBatch() {
            this.items = new List<IItemInstance>();
        }
        public ItemBatch(IItemInstance instance)
        {
            this.items = new List<IItemInstance>();
            items.Add(instance);
        }
        public ItemBatch(List<IItemInstance> instances)
        {
            if (instances.Count == 0)
                return;
            this.items = new List<IItemInstance>(instances);
        }
        public ItemBatch(IItemInstance item, int count = 1)
        {
            this.items = new List<IItemInstance>();

            for (int i = 0; i < count; i++)
            {
                items.Add(item);
            }
        }

        public bool Has(IItemInstance instance)
        {
            return items.Contains(instance);
        }

        public IItemInstance Take(IItemInstance instance)
        {
            if (!items.Contains(instance))
                throw new Exception("Inventory did not have item instance " + instance.ToString());
            items.Remove(instance);
            return instance;
        }

        public List<IItemInstance> Empty()
        {
            List<IItemInstance> items = new List<IItemInstance>(this.items);
            this.items.Clear();
            return items;
        }
    }
}