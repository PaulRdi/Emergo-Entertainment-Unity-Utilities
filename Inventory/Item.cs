﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    [CreateAssetMenu(fileName = "item.asset", menuName = "Crafting/Item")]
    public class Item : ScriptableObject
    {
        public GameObject prefab => _prefab;
        [SerializeField] GameObject _prefab;
        public void SetPrefab(GameObject prefab)
        {
            this._prefab = prefab;
        }

        public bool consumeOnUse => _consumeOnUse;
        [SerializeField] bool _consumeOnUse = true;

        public float stackWeight => _stackWeight;
        [SerializeField] float _stackWeight = 1.0f;

        public Sprite Icon { get => icon; }
        [SerializeField] Sprite icon;
        public void SetIcon(Sprite icon)
        {
            this.icon = icon;
        }

        public string Description { get => description; }
        [SerializeField] [TextArea] string description;

        public static Item Create(string name, GameObject prefab, bool consumeOnUse, float stackWeight, Sprite icon)
        {
            Item inst = ScriptableObject.CreateInstance<Item>();
            inst._consumeOnUse = consumeOnUse;
            inst._stackWeight = stackWeight;
            inst.icon = icon;
            inst._prefab = prefab;
            inst.name = name;
            return inst;
        }


    }
}