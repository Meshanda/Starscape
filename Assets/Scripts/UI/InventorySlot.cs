using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemCount;
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
        print("Refresh" + _itemStack.number);
        _itemImage.sprite = _itemStack?.item.sprite;
        _itemCount.text = _itemStack?.number.ToString();
        
        _itemImage.gameObject.SetActive(_itemStack != null);
        _itemCount.gameObject.SetActive(_itemStack?.number > 1);
    }
}
