using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace EmergoEntertainment.Inventory
{
    [RequireComponent(typeof(Button))]
    public class RecipeButton : MonoBehaviour
    {
        public event Action<RecipeButton> clicked;
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Clicked);
        }

        private void Clicked()
        {
            clicked?.Invoke(this);
        }
    }
}