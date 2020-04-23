using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    [Serializable]
    public class RecipeComponent
    {

        public Item Item { get => component; }
        [SerializeField] Item component;

        public int Amount { get => amount; }
        [SerializeField] int amount;
    }
}
