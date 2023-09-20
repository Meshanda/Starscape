using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Database")]
public class DatabaseSO : ScriptableObject
{
    public Item[] _items;

    public Item GetItemById(string id)
    {
        return _items.FirstOrDefault(item => item.id.Equals(id));
    }
    
    public Item GetItemByTile(TileBase tile)
    {
        return _items.FirstOrDefault(item => item.tile.Equals(tile));
    }
}

[Serializable]
public struct Loot
{
    // public bool requireSilkTouch;
    // public int outil;
    public string itemId;
    public int count;
}

[Serializable]
public class Item
{
    public string id;
    public string name;
    public Sprite sprite;
    public TileBase tile;
    
    public Loot loot;

    public ItemStack GenerateLoot()
    {
        var itemStack = new ItemStack()
        {
            item = GameManager.Instance.database.GetItemById(loot.itemId),
            number = loot.count
        };

        return itemStack;
    }
    
}


