using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class Chest : MonoBehaviour, IPointerDownHandler
    {
        public List<ItemStack> itemStacks = new();

        private Transform _slotParent; 
        public bool ChestOpen { get; private set; }

        private void Start()
        {
            itemStacks.Clear();
            for (int i = 0; i < InventorySystem.Instance._nbChestSlots; i++)
            {
                var stack = new ItemStack
                {
                    itemID = "dirt_01",
                    number = i+1
                };
                
                itemStacks.Add(stack);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ToggleChest(!ChestOpen);
            }
        }

        private void ToggleChest(bool status)
        {
            ChestOpen = status;
            if (status)
            {
                InventorySystem.Instance.OpenChest(itemStacks);
            }
            else
                InventorySystem.Instance.CloseChest();
            
            InventorySystem.Instance.ToggleInventory();
        }
        
    }
}