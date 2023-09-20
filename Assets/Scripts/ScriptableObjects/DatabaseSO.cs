using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Database")]
public class DatabaseSO : ScriptableObject
{
    public Item[] _items;

    public Item GetItemByTile(TileBase tile)
    {
        return _items.FirstOrDefault(item => item.tile.Equals(tile));
    }
}

[Serializable]
public class Item
{
    public string name;
    public Sprite sprite;
    public TileBase tile;
}

