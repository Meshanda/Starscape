using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class Chest : MonoBehaviour
    {
        public List<ItemStack> itemStacks = new();

        private Transform _slotParent; 
        public bool ChestOpen { get; private set; }

        public void PlaceLoot(List<ItemStack> loots)
        {
            itemStacks.Clear();
            foreach (var loot in loots)
            {
                itemStacks.Add(loot);
            }
        }

        public void ClickChest()
        {
            ToggleChest(!ChestOpen);
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