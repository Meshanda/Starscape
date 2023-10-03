using System.Collections;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandSlot : Slot
{
    public override ItemStack ItemStack
    {
        get => base.ItemStack;
        set
        {
            _itemStack = value;

            TooltipSystem.Instance.SetActive(_itemStack is null);

            Refresh();
        }
    }

    private void Update()
    {
        transform.position = Input.mousePosition;
    }
}
