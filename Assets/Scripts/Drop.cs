using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Drop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    private ItemStack _itemStack;
    public ItemStack ItemStack
    {
        get => _itemStack;
        set
        {
            _itemStack = value;
            
            if (_itemStack != null)
            {
                _renderer.sprite = _itemStack.item.sprite;
            }
            else
            {
                print("Should not happen");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.tag.Equals("Player"))
            return;

        if (InventorySystem.Instance.AddItem(ItemStack))
        {
            Destroy(gameObject);
        }
        else
        {
            print("Can't add item to inventory");
        }
    }
}