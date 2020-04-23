using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    [CreateAssetMenu(fileName = "all_items.asset")]
    public class AllItems : ScriptableObject
    {
        public List<Item> items;
        public static AllItems current;
    }
}

