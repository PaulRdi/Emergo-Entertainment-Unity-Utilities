using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    [RequireComponent(typeof(PlayerInventoryManager))]
    public class InventoryTest : MonoBehaviour
    {
        [SerializeField] Item wood, iron;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                PlayerInventoryManager.instance.playerInventory.TryAddItem(wood);
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                PlayerInventoryManager.instance.playerInventory.TryAddItem(iron);
            }
        }
    }
}