using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    public Vector2Int pos;

    private ItemStack _itemStack;

    public void ChangeSlot(ItemStack stack)
    {
        _itemStack = stack;
        _itemImage.sprite = stack?.item.sprite;
        
        _itemImage.gameObject.SetActive(stack is null);
    }
}
