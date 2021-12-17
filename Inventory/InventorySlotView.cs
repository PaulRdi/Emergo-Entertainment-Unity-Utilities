using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

namespace EmergoEntertainment.Inventory
{
    public class InventorySlotView : MonoBehaviour, IPointerClickHandler, IDragHandler, IDropHandler, IPointerUpHandler, IPointerDownHandler
    {
        public static event Action<InventorySlotView> Clicked;
        public static event Action<InventorySlotView> DraggedToTrash;
        public static event Action<InventorySlotView> DragStarted;
        /// <summary>
        /// Called when an 
        /// </summary>
        public static event Action<InventorySlotView, GameObject> DraggedToWorldSpaceObject;
        public static event Action<InventorySlotView, Vector2> DragReleased;

        public static event Func<InventorySlotView ,Camera> eventCameraRequested;

        public event Action<InventorySlotView> clicked;
        public event Action<InventorySlotView, GameObject> draggedToWorldSpaceObject;
        public event Action<InventorySlotView, Vector2> dragReleased;
        public static event Action<InventorySlotView> dragStarted;
        public event Action<InventorySlotView> updated;
        static GameObject dragObject;
        Camera eventCamera;
        [SerializeField] TextMeshProUGUI stackSizeText;
        [SerializeField] Image image;
        /// <summary>
        /// Defines which layers are targeted to trigger the DraggedToObject event.
        /// </summary>
        [SerializeField] LayerMask dragMask;

        bool dragging;
        public IInventoryUI assignedUI { get; private set; }

        public int slotID;

        public void Init(IInventoryUI invManager, Canvas canvas, int id)
        {
            this.assignedUI = invManager;
            if (invManager.TryRegisterSlotView(this, id))
            {
                slotID = id;
            }
            else
            {
                throw new System.Exception("Could not add UI representation of inventory slot.");
            }
            UpdateItemSlot(null);
            dragging = false;

            if (dragObject == null)
            {
                dragObject = new GameObject("Drag Object", typeof(RectTransform), typeof(Image));
                dragObject.transform.SetParent(canvas.transform);
                dragObject.SetActive(false);
                dragObject.GetComponent<Image>().raycastTarget = false;
                dragObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 50.0f);
                dragObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50.0f);
            }
            this.eventCamera = invManager.eventCamera;
        }

        public void UpdateItemSlot(ItemBatch batch)
        {
            if (batch == null)
            {
                image.sprite = null;
                stackSizeText.gameObject.SetActive(false);
                image.color = new Color(1, 1, 1, 0);

            }
            else
            {
                image.sprite = batch.item.Icon;
                image.color = new Color(1, 1, 1, 1);
                stackSizeText.gameObject.SetActive(true);
                stackSizeText.text = batch.count.ToString();
            }
            updated?.Invoke(this);
        }

        private void ButtonClicked()
        {
            clicked?.Invoke(this);
            Clicked?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }
            dragObject.transform.position = eventData.position;

        }

        internal void SetEventCamera(Camera eventCamera)
        {
            this.eventCamera = eventCamera;
        }
        void GetEventCamera()
        {
            if (eventCamera == default &&
                eventCameraRequested != default)
            {
                eventCamera = eventCameraRequested.Invoke(this);
            }
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!dragging)
                return;
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);

            List<RaycastResult> res = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, res);

            dragReleased?.Invoke(this, eventData.position);
            DragReleased?.Invoke(this, eventData.position);

            GetEventCamera();

            if (eventCamera != null)
            {
                ray = eventCamera.ScreenPointToRay(Input.mousePosition);
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
                    draggedToWorldSpaceObject?.Invoke(this, go);
                    DraggedToWorldSpaceObject?.Invoke(this, go);
                    foreach (IInventorySlotInteractable slotInteractable in go.GetComponents<IInventorySlotInteractable>())
                    {
                        slotInteractable.InventorySlotDroppedOnObject(this);
                    }
                }
            }


            dragObject.SetActive(false);
            dragging = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.hovered.Contains(this.gameObject))
            {
                dragging = true;
                dragObject.SetActive(true);
                dragObject.transform.position = eventData.position;
                dragObject.GetComponent<Image>().sprite = image.sprite;
                dragObject.GetComponent<Image>().color = image.color;
                dragStarted?.Invoke(this);
                DragStarted?.Invoke(this);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ButtonClicked();
        }
    }
}