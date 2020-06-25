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
        public event Action<IItemInstance> ItemsAdded;


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
            return !(queriedBatch == default(ItemBatch) && slotToItemBatch.Any(s => s.Value == null));
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
                return queriedBatch.TryAdd(itemInstance);
            }
            else
            {
                if (!slotToItemBatch.Any(s => s.Value == null))
                {
                    return false;
                }
                ItemBatch batch = new ItemBatch(itemInstance);
                itemBatches.Add(batch);
                int freeInventorySlotID = slotToItemBatch.Keys.First(k => slotToItemBatch[k] == null);
                slotToItemBatch[freeInventorySlotID] = batch;
                AddBatch(itemInstance.data, batch);
            }
            UpdateEmptyBatches();
            return true;
        }
        public bool TryAddItem(Item item)
        {
            ItemBatch queriedBatch = itemBatches.FirstOrDefault(b => b.item == item && b.fillLevel < maxBatchSize);

            if (queriedBatch != default(ItemBatch))
            {
                queriedBatch.AddNew(1);
            }
            else
            {
                if (!slotToItemBatch.Any(s => s.Value == null))
                {
                    return false;
                }
                ItemBatch batch = new ItemBatch(ItemManager.CreateItemInstance(item, null), 1);
                itemBatches.Add(batch);
                int freeInventorySlotID = slotToItemBatch.Keys.First(k => slotToItemBatch[k] == null);
                slotToItemBatch[freeInventorySlotID] = batch;
                AddBatch(item, batch);
            }
            UpdateEmptyBatches();
            return true;
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
            items = null;
            if (!slotToItemBatch.ContainsKey(slotID) ||
                slotToItemBatch[slotID] == null)
                return false;
            if (slotToItemBatch[slotID].count <= 0)
                return false;

            items = slotToItemBatch[slotID].Take(1);
            UpdateEmptyBatches();
            return true;
        }

        public int GetTotalItemAmount (Item item)
        {
            return itemToItemBatch.Where(kvp => kvp.Key == item)
                .Sum(kvp => kvp.Value
                .Sum(batch => batch.count));
        }

        public bool TryTakeItems(Item item, out List<IItemInstance> items, int amount = 1)
        {
            items = null;
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

    }
}