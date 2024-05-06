using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.EventSystems;

namespace EmergoEntertainment.Inventory
{
    /// <summary>
    /// Uses pre-defined stylesheet name to create the visual elements for the inventory UI
    /// "DragObject" is the element for the dragged item
    /// </summary>
    public class UIToolkitDragObject : MonoBehaviour
    {
        private UIToolkitPlayerInventoryManager inventoryManager;
        private Inventory inventory => inventoryManager.inventory;


        public static event Action<UIToolkitInventorySlot, GameObject> DraggedToWorldSpaceObject;
        public event Action<UIToolkitInventorySlot, GameObject> draggedToWorldSpaceObject;

        [SerializeField] private LayerMask dragMask;

        private VisualElement root;
        private VisualElement dragObject;

        private bool dragging = false;

        [Header("Settings")]
        [SerializeField] private float dragObjectSize = 100;
        [SerializeField] private bool autoDragOffset = true;

        private void Start()
        {
            inventoryManager = FindFirstObjectByType<UIToolkitPlayerInventoryManager>();

            CreateDragObject();
        }

        private void CreateDragObject() 
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            dragObject = new VisualElement();
            dragObject.style.width = dragObjectSize;
            dragObject.style.height = dragObjectSize;
            dragObject.AddToClassList("DragObject");
            dragObject.pickingMode = PickingMode.Ignore;
            root.Add(dragObject);

            dragObject.style.display = DisplayStyle.None;
        }

        private void OnEnable()
        {
            UIToolkitInventorySlot.OnDragStart += StartDrag;
            UIToolkitInventorySlot.OnDragEnd += StopDrag;
        }

        private void OnDisable()
        {
            UIToolkitInventorySlot.OnDragStart -= StartDrag;
            UIToolkitInventorySlot.OnDragEnd -= StopDrag;
        }

        private void Update()
        {
            SetDragObjectToMousePosition();
        }

        private void SetDragObjectToMousePosition()
        {
            if (!dragging)
                return;

            Vector2 mousePosition = Input.mousePosition;
            mousePosition.y = Screen.height - mousePosition.y;

            Vector2 localMousePosition = root.ChangeCoordinatesTo(root.parent, mousePosition);

            float offset = autoDragOffset ? dragObjectSize / 2 : 0;

            dragObject.style.left = localMousePosition.x - offset;
            dragObject.style.top = localMousePosition.y - offset;
        }

        private void StartDrag(UIToolkitInventorySlot slot)
        {
            var batch = inventory.slotToItemBatch[slot.slotID];
            if (batch == null || batch.item == null)
                return;

            var item = batch.item;
            dragging = true;
            dragObject.style.display = DisplayStyle.Flex;
            dragObject.style.backgroundImage = new StyleBackground(item.Icon);
        }

        private void StopDrag(UIToolkitInventorySlot slot)
        {
            dragObject.style.display = DisplayStyle.None;
            var batch = inventory.slotToItemBatch[slot.slotID];
            if (batch == null || batch.item == null || !dragging)
                return;

            dragging = false;

            CheckForWorldSpaceDrag(slot);
            CheckForInventorySlotDrag(slot);
        }

        private void CheckForWorldSpaceDrag(UIToolkitInventorySlot slot)
        {
            List<RaycastResult> res = new List<RaycastResult>();
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, dragMask);
            RaycastHit2D[] hits2D = Physics2D.GetRayIntersectionAll(ray, float.MaxValue, dragMask);

            IEnumerable<GameObject> intersectedObjects =
                hits.Select(h => h.collider.gameObject)
            .Union(
                    hits2D.Select(h2d => h2d.collider.gameObject))
                .Union(
                    res.Select(resHit => resHit.gameObject));


            foreach (GameObject go in intersectedObjects)
            {
                draggedToWorldSpaceObject?.Invoke(slot, go);
                DraggedToWorldSpaceObject?.Invoke(slot, go);
            }
        }

        private void CheckForInventorySlotDrag(UIToolkitInventorySlot slot)
        {
            var hoveringSlot = UIToolkitInventorySlot.hoveringSlot;
            if (hoveringSlot == null || hoveringSlot == slot)
                return;

            hoveringSlot.TryMoveItemFromOtherInventorySlotToThis(slot);
        }
    }
}