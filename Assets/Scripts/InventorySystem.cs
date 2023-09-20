using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStack
{
    public Item item;
    public int number;
    public Vector2Int position;
}

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private List<InventorySlot> _slots;

    private void Awake()
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

    public void AddItem(Drop drop)
    {
        var stack = new ItemStack
        {
            item = drop.item,
            number = drop.number
        };

        AddItem(stack);
    }

    public void AddItem(ItemStack itemStack)
    {
        GetClosestSlot();
    }

    private void GetClosestSlot()
    {
        
    }
}
