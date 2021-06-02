using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace EmergoEntertainment.Inventory
{
    public class Inventory
    {
        public event Action Updated;
        public event Action<IItemInstance> ItemAdded;

        public Dictionary<Item, List<ItemBatch>> itemToItemBatch;
        public List<ItemBatch> itemBatches;
        public Dictionary<int, ItemBatch> slotToItemBatch;
        public int maxBatchSize { get; private set; }      

        public Inventory(int maxBatchSize, int numSlots)
        {
            itemToItemBatch = new Dictionary<Item, List<ItemBatch>>();
            slotToItemBatch = new Dictionary<int, ItemBatch>();
            itemBatches = new List<ItemBatch>();
            this.maxBatchSize = maxBatchSize;
            for (int i = 0; i < numSlots; i++)
                slotToItemBatch.Add(i, null);
        }

        public int GetResourceAmount(Item item)
        {
            if (!itemToItemBatch.ContainsKey(item))
                return 0;

            return itemToItemBatch[item].Sum(b => b.count);
        }
        public bool HasItem(Item item, int amount = 1)
        {
            bool checkBatches = itemBatches
                .Where(batch => batch.item == item)
                .Sum(batch => batch.count)
                    >= amount;

            return amount == 0 ? true : checkBatches;
                
        }

        public bool HasItemInstance(IItemInstance itemInstance)
        {
            return itemBatches
                .Where(batch => batch.item == itemInstance.data)
                .Any(batch => batch.items.Contains(itemInstance));
        }

        public bool Query(Item item, out HashSet<IItemInstance> queriedInstances, int amount = 1)
        {
            queriedInstances = new HashSet<IItemInstance>();
            if (!HasItem(item, amount))
                return false;

            int toPut = amount;
            foreach (ItemBatch batch in itemBatches.Where(batch => batch.item == item))
            {
                int toTakeAmount = Math.Min(toPut, batch.count);
                for (int i = 0; i < toTakeAmount; i++)
                {
                    queriedInstances.Add(batch.items[i]);
                }
                if (toTakeAmount == 0)
                    break;
            }
            return true;                
        }

        public void ConsumeItems(Item item, int amount)
        {
            List<ItemBatch> batches = itemBatches
                .FindAll(batch => batch.item == item)
                .OrderBy(batch => batch.count)
                .ToList();

            int itemCount = batches.Sum(b => b.count);
            if (amount > itemCount)
                throw new System.Exception("Cannot consume more items than in inventory");

            int currentAmount = amount;
            int i = 0;
            int safety = 0;
            while (currentAmount > 0)
            {
                if (i < batches.Count)
                {
                    currentAmount -= batches[i].UseMultiple(currentAmount).Count;
                }
                if (batches[i].count <= 0)
                {
                    i++;
                }

                if (safety > itemCount)
                    throw new System.Exception("Inventory consume went out of bounds!");
                safety++;
            }
            UpdateEmptyBatches();
        }

        public void CraftItem(Recipe toCraft)
        {
            foreach (RecipeComponent c in toCraft.Components)
            {
                ConsumeItems(c.Item, c.Amount);
            }

            TryAddItem(toCraft.Result);
        }

        public bool CanAddItem(Item item)
        {
            ItemBatch queriedBatch = itemBatches.FirstOrDefault(b => b.item == item && b.count < maxBatchSize);
            return queriedBatch != default(ItemBatch) || slotToItemBatch.Any(s => s.Value == null);
        }
        public bool TryAddItemInstance(IItemInstance itemInstance)
        {
            ItemBatch queriedBatch = itemBatches.
                FirstOrDefault(
                b =>
                b.item == itemInstance.data &&
                b.fillLevel <= maxBatchSize - b.item.stackWeight);
            if (queriedBatch != default(ItemBatch))
            {
                if (queriedBatch.TryAdd(itemInstance))
                {
                    ItemAdded?.Invoke(itemInstance);
                    return true;
                }
                return false;
            }
            else
            {
                if (!slotToItemBatch.Any(s => s.Value == null))
                {
                    return false;
                }
                CreateNewItemBatch(itemInstance.data, itemInstance);
                ItemAdded?.Invoke(itemInstance);
                return true;
            }
        }
        public bool TryAddItem(Item item)
        {
            ItemBatch queriedBatch = itemBatches.FirstOrDefault(
                b =>
                b.item == item &&
                b.fillLevel < maxBatchSize - item.stackWeight);
            IItemInstance itemInstance = default;
            if (queriedBatch != default(ItemBatch))
            {
                itemInstance = queriedBatch.AddNew(1)[0];
                ItemAdded?.Invoke(itemInstance);
                return true;
            }
            else
            {
                if (!slotToItemBatch.Any(s => s.Value == null))
                {
                    return false;
                }
                itemInstance = ItemManager.CreateItemInstance(item, null);
                CreateNewItemBatch(item, itemInstance);
                UpdateEmptyBatches();
                ItemAdded?.Invoke(itemInstance);
                return true;
            }
        }

        private ItemBatch CreateNewItemBatch(Item item, IItemInstance itemInstance, int slotId = -1)
        {
            if (slotToItemBatch.All(s => s.Value != null && s.Value.item != default))
                return default;
            ItemBatch batch = new ItemBatch(itemInstance, 1);
            itemBatches.Add(batch);
            if (slotId == -1)
            {
                int freeInventorySlotID = slotToItemBatch.Keys.First(k => slotToItemBatch[k] == null);
                slotToItemBatch[freeInventorySlotID] = batch;
            }
            else
            {
                if (slotId < 0)
                slotToItemBatch[slotId] = batch;
            }
            AddBatch(item, batch);
            UpdateEmptyBatches();
            return batch;
        }

        private void AddBatch(Item item, ItemBatch batch)
        {
            if (!itemToItemBatch.ContainsKey(item))
            {
                itemToItemBatch.Add(item, new List<ItemBatch>());
            }
            itemToItemBatch[item].Add(batch);
            UpdateEmptyBatches();
        }


       public bool TryTakeItemFromSlot(int slotID, out List<IItemInstance> items)
        {
            items = new List<IItemInstance>();
            if (!slotToItemBatch.ContainsKey(slotID) ||
                slotToItemBatch[slotID] == null)
                return false;
            if (slotToItemBatch[slotID].count <= 0)
                return false;

            items = slotToItemBatch[slotID].Take(1);
            UpdateEmptyBatches();
            return true;
        }

        public bool TryAddItemInstanceToSlot(int slotID, IItemInstance itemInstance)
        {
            if (!slotToItemBatch.ContainsKey(slotID))
                return false;
            if (HasItemInstance(itemInstance))
                return false;

            ItemBatch batch = slotToItemBatch[slotID];


            if (batch == default)
            {
                slotToItemBatch[slotID] = CreateNewItemBatch(itemInstance.data, itemInstance, slotID);
                return true;
            }
            else if (batch.item != itemInstance.data)
                return false;

            return batch.TryAdd(itemInstance);
        }

        public int GetTotalItemAmount (Item item)
        {
            return itemToItemBatch.Where(kvp => kvp.Key == item)
                .Sum(kvp => kvp.Value
                .Sum(batch => batch.count));
        }

        public bool TryTakeItems(Item item, out List<IItemInstance> items, int amount = 1)
        {
            items = new List<IItemInstance>();

            if (GetTotalItemAmount(item) < amount)
                return false;

            for (int i = 0; i < amount; i++)
            {
                ItemBatch batch = itemBatches.First(b => b.item == item);
                items.Add(batch.Take(1)[0]);
            }
            UpdateEmptyBatches();
            return true;
        }
        /// <summary>
        /// Tries to remove a specific IItemInstance from this inventory
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="itemInstance"></param>
        /// <returns></returns>
        public bool TryTakeItemInstance(IItemInstance instance)
        {
            ItemBatch batch = itemBatches.FirstOrDefault(b => b.Has(instance));
            if (batch == default)
            {
                return false;
            }
            batch.Take(instance);
            UpdateEmptyBatches();
            return true;
        }

        public bool TryGiveItems(List<Item> items, out List<Item> leftoverItems)
        {
            leftoverItems = new List<Item>();
            foreach (Item item in items)
            {
                if (!TryAddItem(item))
                {
                    leftoverItems.Add(item);
                }
            }
            UpdateEmptyBatches();
            return leftoverItems.Count <= 0;
        }

        public void UpdateEmptyBatches()
        {
            //remove items from the item batch dictionary if there are no batches with this item left.
            foreach (Item i in itemToItemBatch.Keys)
            {
                itemToItemBatch[i].RemoveAll(b => b.count <= 0);
            }
            itemToItemBatch.RemoveAll((k, v) => v.Count == 0);
            foreach (int sk in slotToItemBatch.Keys.ToList())
            {
                if (slotToItemBatch[sk] == null) continue;
                if (slotToItemBatch[sk].count == 0)
                    slotToItemBatch[sk] = null;
            }
            itemBatches.RemoveAll(i => i.count <= 0);

            Updated?.Invoke();
        }

        public List<Item> GetBestItemByCondition(Func<Item, float> orderbyParameter)
        {
            List<Item> output = new List<Item>();
            output = itemToItemBatch.Keys.OrderBy(orderbyParameter).ToList();
            return output;
        }

        public List<IItemInstance> Empty()
        {
            itemToItemBatch.Clear();
            List<IItemInstance> items = new List<IItemInstance>();
            foreach(ItemBatch batch in itemBatches)
            {
                items.AddRange(batch.Empty());
            }

            UpdateEmptyBatches();
            return items;
        }
    }
}