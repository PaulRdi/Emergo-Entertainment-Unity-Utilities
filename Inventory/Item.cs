using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    [CreateAssetMenu(fileName = "item.asset", menuName = "Crafting/Item")]
    public class Item : ScriptableObject
    {
        public GameObject prefab => _prefab;
        [SerializeField] GameObject _prefab;

        public bool consumeOnUse => _consumeOnUse;
        [SerializeField] bool _consumeOnUse = true;

        public float stackWeight => _stackWeight;
        [SerializeField] float _stackWeight = 1.0f;

        public Sprite Icon { get => icon; }
        [SerializeField] Sprite icon;

        public string Description { get => description; }
        [SerializeField] [TextArea] string description;


    }
}