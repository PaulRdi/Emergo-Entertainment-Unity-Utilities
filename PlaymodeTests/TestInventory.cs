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
            ItemManager itemManager = new ItemManager();
            Inventory.Inventory inventory = new Inventory.Inventory(5, 2);
            Item item1 = ScriptableObject.CreateInstance<Item>();
            item1.name = "item1";

            Item item2 = ScriptableObject.CreateInstance<Item>();
            item2.name = "item2";


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
            List<IItemInstance> items = new List<IItemInstance>();
            ItemManager itemManager = new ItemManager();
            Inventory.Inventory inventory = new Inventory.Inventory(5, 2);
            Item item1 = ScriptableObject.CreateInstance<Item>();
            item1.name = "item1";
            Item item2 = ScriptableObject.CreateInstance<Item>();
            item2.name = "item2";

            Assert.True(inventory.TryAddItem(item1));
            Assert.True(inventory.TryAddItem(item2));
            Assert.True(inventory.TryAddItem(item2));
            yield return null;
            Assert.False(inventory.TryTakeItems(item1, out items, 2));
            Assert.True(inventory.TryTakeItems(item1, out items, 1));
            Assert.False(inventory.TryTakeItems(item1, out items, 1));
            Assert.True(inventory.TryTakeItems(item2, out items, 2));
            Assert.AreEqual(3, items.Count);
        }

        [UnityTest]
        public IEnumerator TestAddAndTakeItems()
        {
            List<IItemInstance> items = new List<IItemInstance>();
            ItemManager itemManager = new ItemManager();
            Inventory.Inventory inventory = new Inventory.Inventory(5, 2);
            Item item1 = ScriptableObject.CreateInstance<Item>();
            item1.name = "item1";
            Item item2 = ScriptableObject.CreateInstance<Item>();
            item2.name = "item2";
            Item item3 = ScriptableObject.CreateInstance<Item>();
            item3.name = "item3";

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



    }
}
