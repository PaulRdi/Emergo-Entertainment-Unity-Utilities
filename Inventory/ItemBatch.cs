using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
namespace EmergoEntertainment.Inventory
{
    public class ItemBatch
    {
        public readonly Item item;
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
        public void Add(int amount)
        {
            items.Add(ItemManager.CreateItemInstance(item));
        }
        public void Add(IItemInstance itemInstance)
        {
            items.Add(itemInstance);
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
            this.item = instance.data;
            items.Add(instance);
        }
        public ItemBatch(List<IItemInstance> instances)
        {
            if (instances.Count == 0)
                return;
            this.item = instances[0].data;
            this.items = new List<IItemInstance>(instances);
        }
        public ItemBatch(Item item, int count = 1)
        {
            this.items = new List<IItemInstance>();
            this.item = item;

            for (int i = 0; i < count; i++)
            {
                IItemInstance instance = ItemManager.CreateItemInstance(item);
                items.Add(instance);

            }
        }
    }
}