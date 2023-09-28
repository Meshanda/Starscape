using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Database")]
public class DatabaseSO : ScriptableObject
{
    [FormerlySerializedAs("_items")] public Item[] items;
    public List<CraftRecipe> craftRecipes;

    public Item GetItemById(string id)
    {
        return items.FirstOrDefault(item => item.id.Equals(id));
    }
    
    public Item GetItemByTile(TileBase tile)
    {
        return items.FirstOrDefault(item => item.tile.Equals(tile));
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
    public string description;
    public Sprite sprite;
    public TileBase tile;
    
    public Loot loot;

    public ItemStack GenerateLoot()
    {
        var itemStack = new ItemStack
        {
            itemID = loot.itemId,
            number = loot.count
        };

        return itemStack;
    }
}

[Serializable]
public struct CraftRecipe
{
    public List<ItemStack> itemsRequired;
    public ItemStack itemCrafted;
}
