using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Drop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    public ItemStack itemStack;

    private void Awake()
    {
        _renderer.sprite = itemStack.item.sprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player"))
            return;

        if (InventorySystem.Instance.AddItem(itemStack))
        {
            Destroy(gameObject);
        }
    }
}