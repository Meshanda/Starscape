using System;
using UnityEngine;

public class Drop : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    
    [Header("Debug")]
    public Item item;
    public int number;

    private void Awake()
    {
        _renderer.sprite = item.sprite;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player"))
            return;
        
        // Code TODO lol
    }
}