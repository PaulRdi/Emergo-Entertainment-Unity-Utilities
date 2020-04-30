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

    }
}
