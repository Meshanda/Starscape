using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class Chest : MonoBehaviour
    {
        public List<ItemStack> itemStacks = new();
        public float distance = 2;

        private Transform _slotParent; 
        public bool ChestOpen { get; set; }

        private void Start()
        {
            for (int i = 0; i < InventorySystem.Instance._nbChestSlots; i++)
            {
                itemStacks.Add(default);
            }
        }

        public void PlaceLoot(List<ItemStack> loots)
        {
            itemStacks.Clear();
            foreach (var loot in loots)
            {
                itemStacks.Add(loot);
            }
        }

        public void UpdateChest(List<InventorySlot> loots)
        {
            itemStacks.Clear();
            foreach (var loot in loots)
            {
                itemStacks.Add(loot.ItemStack);
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
                InventorySystem.Instance.OpenChest(this);
            }
            else
            {
                InventorySystem.Instance.CloseChest(this);
                InventorySystem.Instance.ToggleInventory();
            }
        }

        private bool _isQuitting = false;
        
        void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if (!_isQuitting && gameObject.scene.isLoaded)
            {
                // drop everything that is in the chest
                foreach (var itemStack in itemStacks)
                {
                    if (ItemStack.IsValid(itemStack))
                    {
                        World.Instance.GenerateDrop(itemStack.Clone(), transform.position).AddRandomForce();
                    }
                }
                
                InventorySystem.Instance.CloseChest(this);
            }
        }
    }
}