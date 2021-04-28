using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EmergoEntertainment.Inventory
{
    public interface IItemInstance
    {
        Item data { get; }
        GameObject gameObject { get; }
        T GetController<T>() where T : MonoBehaviour, IItemBehaviour;
        T GetData<T>() where T : Item;
        bool TryGetData<T>(out T data) where T : Item;
        void Cleanup();
        bool UseFromInventory();
        bool Drop();
    }
}