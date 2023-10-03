using System;
using System.Collections.Generic;
using System.Linq;
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
        return items.FirstOrDefault(item => item.tileInfo.tile && item.tileInfo.tile.Equals(tile));
    }
}

[Serializable]
public struct ToolStrength
{
    // public ToolEffect effect; // silk touch...
    public ToolType type;
    public int level;
    
    public bool IsEnoughFor(ToolStrength strength)
    {
        return level >= strength.level && (((type & strength.type) != 0) || strength.type == 0);
    }
    
    public static ToolStrength DEFAULT => new ToolStrength()
    {
        type = 0,
        level = 0,
    };
}

[Serializable]
public struct ToolData
{
    public ToolStrength toolStrength;
    public float tileDamagePerSecond;
    // public float entityDamage; // TODO?
    // public float durability; // TODO?

    public static ToolData DEFAULT => new ToolData()
    {
        toolStrength = ToolStrength.DEFAULT,
        tileDamagePerSecond = 1.0f,
    };
}

[Flags]
public enum ToolType
{
    None        = 0, // basically hands or not a tool
    Pickaxe     = 1 << 1,
    Axe         = 1 << 2,
    Hammer      = 1 << 3,
}

[Serializable]
public struct TileInfo
{
    public TileBase tile;
    public float life;
    public ToolStrength requiredToBreak;
    public ItemStack lootOnBreak;
    
    public bool HasAnyLoot()
    {
        return lootOnBreak?.GetItem() != null && lootOnBreak.number > 0;
    }
    
    public ItemStack GenerateLoot()
    {
        return lootOnBreak.Clone();
    }
    
    public static TileInfo DEFAULT => new TileInfo()
    {
        tile = null,
        life = 1.0f,
        requiredToBreak = ToolStrength.DEFAULT,
        lootOnBreak = null,
    };
}

[Serializable]
public class Item
{
    public string id;
    public string name;
    public string description;
    public Sprite sprite;
    public TileInfo tileInfo = TileInfo.DEFAULT;
    public ToolData toolData = ToolData.DEFAULT;
}
    
[Flags]
public enum CraftingFlags
{
    CraftingTable   = 1 << 0,
    Furnace         = 1 << 1,
    // ...
}

[Serializable]
public struct CraftRecipe
{
    public List<ItemStack> itemsRequired;
    public ItemStack itemCrafted;
    public CraftingFlags requiredFlags;

    public bool CanBeCrafted()
    {
        var inventory = InventorySystem.Instance;

        if (requiredFlags != 0 && ((inventory.CraftingFlags & requiredFlags) == 0))
        {
            return false;
        }
        
        foreach (var item in itemsRequired)
        {
            if (inventory.CountItem(item.itemID) < item.number)
            {
                return false;
            }
        }

        return true;
    }
}
