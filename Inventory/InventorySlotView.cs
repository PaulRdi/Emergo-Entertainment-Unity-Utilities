using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

namespace EmergoEntertainment.Inventory
{
    [RequireComponent(typeof(Button))]
    public class InventorySlotView : MonoBehaviour, IDragHandler, IDropHandler, IPointerUpHandler, IPointerDownHandler
    {
        public static event Action<InventorySlotView> Clicked;
        //Reminder: We need some sort of hook.
        //public static event Action<InventorySlot, FiresidePlace> DraggedToCampsite;
        //public static event Action<InventorySlot, PlayerController> DraggedToPlayer;
        public static event Action<InventorySlotView> DraggedToTrash;
        static GameObject dragObject;

        [SerializeField] TextMeshProUGUI stackSizeText;
        [SerializeField] Image image;

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
            GetComponent<Button>().onClick.AddListener(ButtonClicked);
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


        public void OnPointerUp(PointerEventData eventData)
        {
            if (!dragging)
                return;
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            //Todo: Make it so item can be dragged to a target

            List<RaycastResult> res = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, res);
            if (res.Any(r => r.gameObject == PlayerInventoryManager.instance.trashObject))
                DraggedToTrash?.Invoke(this);

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
    }
}