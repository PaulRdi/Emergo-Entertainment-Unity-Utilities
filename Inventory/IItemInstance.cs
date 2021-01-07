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
        void Cleanup();
        bool UseFromInventory();
        bool Drop();
    }
}