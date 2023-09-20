using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public Item item;
    public int number;
}

public class InventorySystem : Singleton<InventorySystem>
{
    [SerializeField] private List<InventorySlot> _slots;

    protected override void SingletonAwake()
    {
        InitSlots();
    }

    private void InitSlots()
    {
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].pos = new Vector2Int(0,i);
        }
    }

    public bool AddItem(ItemStack itemStack)
    {
        var slot = GetClosestSlot(itemStack.item);
        if (!slot)
            return false;

        if (slot.ItemStack == null)
        {
            slot.ItemStack = itemStack;
        }
        else
        {
            slot.ItemStack.number += itemStack.number;
        }

        return true;
    }

    private InventorySlot GetClosestSlot(Item item)
    {
        foreach (var slot in _slots)
        {
            if (slot.ItemStack == null || slot.ItemStack.item == item)
            {
                return slot;
            }
        }

        return null;
    }
}
