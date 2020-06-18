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
        //Reminder: We need some sort of hook.
        //public static event Action<InventorySlot, FiresidePlace> DraggedToCampsite;
        //public static event Action<InventorySlot, PlayerController> DraggedToPlayer;
        public static event Action<InventorySlotView> DraggedToTrash;
        public static event Action<InventorySlotView, GameObject> DraggedToWorldSpaceObject;
        static GameObject dragObject;
        Camera eventCamera;
        [SerializeField] TextMeshProUGUI stackSizeText;
        [SerializeField] Image image;

        /// <summary>
        /// Defines which layers are targeted to trigger the DraggedToObject event.
        /// </summary>
        [SerializeField] LayerMask dragMask;

        bool dragging;


        public int slotID
        {
            get; private set;
        }

        public void Init()
        {
            if (FindObjectOfType<PlayerInventoryManager>().TryRegisterSlotView(this, out int id))
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
                dragObject.transform.SetParent(PlayerInventoryManager.instance.transform);
                dragObject.SetActive(false);
                dragObject.GetComponent<Image>().raycastTarget = false;
                dragObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 50.0f);
                dragObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50.0f);
            }

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
        }

        private void ButtonClicked()
        {
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
            if (eventCamera == null)
                eventCamera = FindObjectOfType<Camera>();

            this.eventCamera = eventCamera;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!dragging)
                return;
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            //Todo: Make it so item can be dragged to a target

            List<RaycastResult> res = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, res);
            if (PlayerInventoryManager.instance.trashObject != null &&
                res.Any(r => r.gameObject == PlayerInventoryManager.instance.trashObject.gameObject))
                DraggedToTrash?.Invoke(this);



            if (eventCamera != null &&                
                Physics.Raycast(
                eventCamera.ScreenPointToRay(Input.mousePosition), 
                out RaycastHit hit, 
                Mathf.Infinity, 
                dragMask))
            {
                DraggedToWorldSpaceObject?.Invoke(this, hit.transform.gameObject);
                foreach(IInventorySlotInteractable slotInteractable in hit.transform.gameObject.GetComponentsInChildren<IInventorySlotInteractable>())
                {
                    slotInteractable.InventorySlotDroppedOnObject(this);
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