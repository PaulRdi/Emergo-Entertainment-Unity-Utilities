using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EmergoEntertainment.Inventory;

namespace EmergoEntertainment.UnityUtilityPlaymodetests
{
    public class TestInventory
    {
        ItemManager itemManager;
        Item item1, item2, item3;
        Inventory.Inventory inventory;


        [UnityTest]
        public IEnumerator TestItemAdd()
        {
            yield return null;
            ItemManager itemManager = new ItemManager();
            Inventory.Inventory inventory = new Inventory.Inventory(1, 1);
            Item item = ScriptableObject.CreateInstance<Item>();
            item.name = "test item";

            Assert.IsTrue(inventory.TryAddItem(item), "Could not add item to inventory");
            
        }

        [UnityTest]
        public IEnumerator TestBatchFull()
        {
            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item1));
            yield return null;

            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item1));

            yield return null;

            Assert.False(inventory.TryAddItem(item2));          

        }

        [UnityTest]
        public IEnumerator TestTakeMultipleItems()
        {
            List<IItemInstance> items;
            

            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item2));
            Assert.True(inventory.TryAddItem(item2));
            yield return null;
            Assert.False(inventory.TryTakeItems(item1, out items, 2));
            Assert.True(inventory.TryTakeItems(item1, out items, 1));
            Assert.False(inventory.TryTakeItems(item1, out items, 1));


            Assert.True(inventory.TryTakeItems(item2, out items, 2));
            Assert.AreEqual(items.Count, 2);
        }

        [UnityTest]
        public IEnumerator TestAddAndTakeItems()
        {
            List<IItemInstance> items;
            yield return null;

            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item2));
            Assert.True(inventory.TryAddItem(item2));

            Assert.False(inventory.TryTakeItems(item3, out items));
            Assert.True(inventory.TryTakeItems(item1, out items));
            //try to add the same item instance twice
            Assert.True(inventory.TryAddItemInstance(items[0]));
            Assert.False(inventory.TryAddItemInstance(items[0]));
            Assert.True(inventory.TryAddItem(item1));
        }
        [UnityTest]
        public IEnumerator TestTakeItemInstance()
        {
            yield return null;

            IItemInstance inst1 = ItemManager.CreateItemInstance(item1);
            IItemInstance inst2 = ItemManager.CreateItemInstance(item1);
            IItemInstance inst3 = ItemManager.CreateItemInstance(item2);

            Assert.True(inventory.TryAddItemInstance(inst1));

            //cant take item instances if not in inventory but same data in inventory
            Assert.False(inventory.TryTakeItemInstance(inst2));
            Assert.False(inventory.TryTakeItemInstance(inst3));

            //can take item instance if in inventory
            Assert.True(inventory.TryTakeItemInstance(inst1));

            //cant take item instance twice
            Assert.False(inventory.TryTakeItemInstance(inst1));

            //cant take item instance after it has been taken but another instance of the same data is in inventory
            Assert.True(inventory.TryAddItemInstance(inst1));
            Assert.True(inventory.TryAddItemInstance(inst2));
            Assert.True(inventory.TryTakeItemInstance(inst1));
            Assert.False(inventory.TryTakeItemInstance(inst1));

            //can take item instance if other item instance of same data has been just taken successfully
            Assert.True(inventory.TryTakeItemInstance(inst2));


        }

        [UnityTest]
        public IEnumerator TestQueryForItem()
        {
            inventory.TryAddItem(item1);
            Assert.True(inventory.HasItem(item1));
            Assert.False(inventory.HasItem(item1, 2));
            inventory.TryTakeItems(item1, out List<IItemInstance> taken);
            Assert.False(inventory.HasItem(item1));
            yield return null;

        }

        [UnityTest]
        public IEnumerator TestQueryForItemInstance()
        {
            IItemInstance inst = ItemManager.CreateItemInstance(item1);

            inventory.TryAddItemInstance(inst);
            Assert.True(inventory.HasItemInstance(inst));
            inventory.TryTakeItemInstance(inst);
            Assert.False(inventory.HasItemInstance(inst));
            yield return null;
        }
        [SetUp]
        public void SetUp()
        {
            itemManager = new ItemManager();
            inventory = new Inventory.Inventory(5, 2);
            item1 = ScriptableObject.CreateInstance<Item>();
            item1.name = "item1";
            item2 = ScriptableObject.CreateInstance<Item>();
            item2.name = "item2";
        }

        
    }
}
