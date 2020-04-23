using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace EmergoEntertainment.Inventory
{
    [CreateAssetMenu(fileName = "recipe.asset", menuName = "Crafting/Recipe")]
    public class Recipe : ScriptableObject
    {
        public List<RecipeComponent> Components { get => components; }
        [SerializeField] List<RecipeComponent> components;

        public Item Result { get => result; }
        [SerializeField] Item result;

    }
}