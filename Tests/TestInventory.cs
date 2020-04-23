using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EmergoEntertainment.Inventory;
using System.Reflection;
using System.Linq;
namespace Tests
{
    public class TestInventory
    {
        // A Test behaves as an ordinary method
        [UnityTest]
        public IEnumerator TestInventoryCreation()
        {
            yield return null;
            Inventory inventory = new Inventory(1, 1);
            Assert.NotNull(inventory);

            //Check if all member fields of inventory have been initialized
            Assert.IsTrue(inventory.GetType().GetFields()
                .Select(fi => fi.GetValue(inventory))
                .All(val => val != null),
                "Did not initialize all field members of inventory in constructor.");

            Assert.IsTrue(inventory.GetType().GetProperties()
                .Select(pi => pi.GetValue(inventory))
                .All(val => val != null),
                "Did not initialize all property members of inventory in constructor.");

        }
    }
}
