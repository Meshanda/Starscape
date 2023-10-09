using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class RocketPopUp : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private InventorySlot _slot;
        [SerializeField] private GameObject _button;
        [SerializeField] private float _distance = 3;
        [SerializeField] private GameObject _rocket;

        private void Update()
        {
            if (Vector2.Distance(GameManager.Instance.player.transform.position, _rocket.transform.position) >= _distance)
            {
                GameManager.Instance.ToggleRocketPopUp(); 
            }
        }

        private void OnEnable()
        {
            _slot.slotChanged += OnSlotChanged;
        }

        private void OnDisable()
        {
            _slot.slotChanged -= OnSlotChanged;
        }

        private void OnSlotChanged(ItemStack stack)
        {
            _button.SetActive(_slot.ItemStack?.itemID == "rocket_engine");
        }

        public void ClickButton()
        {
            GameManager.Instance.Win();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            InventorySystem.Instance.OnPointerDown(eventData);
        }
    }
}