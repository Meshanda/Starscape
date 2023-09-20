using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [HideInInspector] public Vector2Int pos;

    private ItemStack _itemStack;
    public ItemStack ItemStack
    {
        get => _itemStack;
        set
        {
            _itemStack = value;
            Refresh();
        }
    }

    private void Refresh()
    {
        print("Refresh" + _itemStack);
        _itemImage.sprite = _itemStack?.item.sprite;
        _itemImage.gameObject.SetActive(_itemStack != null);
    }
}
