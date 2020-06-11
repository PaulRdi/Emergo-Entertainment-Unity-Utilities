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
        public void AddNew(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                items.Add(ItemManager.CreateItemInstance(item));
            }
        }
        public bool TryAdd(IItemInstance itemInstance)
        {
            if (item != itemInstance.data)
                return false;
            if (items.Any(i => i == itemInstance))
                return false;
            items.Add(itemInstance);
            return true;
        }
        public List<IItemInstance> Take(int amount)
        {
            int realAmount = Mathf.Clamp(amount, 0, count);
            List<IItemInstance> output = items.GetRange(0, realAmount);
            items.RemoveRange(0, realAmount);
            return output;
        }
        /// <summary>
        /// Returns the amount of items which were consumed.
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
            inst.Cleanup();

        }
        public bool TryUseSingle(IItemInstance inst)
        {
            if (!items.Contains(inst))
                return false;
            items.Remove(inst);
            inst.Cleanup();
            return true;
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
    }
}