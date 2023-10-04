using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObjects/Database")]
public class DatabaseSO : ScriptableObject
{
    [Space(20)]
    public List<Item> items;
    
    [Space(20)]
    public List<CraftRecipe> craftRecipes;

    public Item GetItemById(string id)
    {
        return items.FirstOrDefault(item => item.id.Equals(id));
    }
    
    public Item GetItemByTile(TileBase tile)
    {
        return items.FirstOrDefault(item => item.tileInfo.tile && item.tileInfo.tile.Equals(tile));
    }

    private void OnValidate()
    {
        List<string> IDs = new();
        foreach (var item in items)
        {
            if (!IDs.Contains(item.id))
            {
                IDs.Add(item.id);
            }
            else
            {
                item.id = "";
            }
        }
        
        items.ForEach(i => i.OnValidate(this));
        craftRecipes.ForEach(r => r.OnValidate(this));
    }
}

[Serializable]
public class ToolStrength
{
    [HideInInspector] public string inspectorName;
    
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
    
    public void OnValidate(DatabaseSO database)
    {
        inspectorName = type + " (lvl " + level + ")";
    }

    protected bool Equals(ToolStrength other)
    {
        return type == other.type && level == other.level;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ToolStrength) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) type, level);
    }

    public static bool operator ==(ToolStrength left, ToolStrength right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ToolStrength left, ToolStrength right)
    {
        return !Equals(left, right);
    }
}

[Serializable]
public class ToolData
{
    [HideInInspector] public string inspectorName;
    
    public ToolStrength toolStrength;
    public float tileDamagePerSecond;
    // public float entityDamage; // TODO?
    // public float durability; // TODO?

    public static ToolData DEFAULT => new ToolData()
    {
        toolStrength = ToolStrength.DEFAULT,
        tileDamagePerSecond = 1.0f,
    };
    
    public void OnValidate(DatabaseSO database)
    {
        toolStrength.OnValidate(database);
        inspectorName = toolStrength.inspectorName + " (dmg " + tileDamagePerSecond + ")";
    }

    protected bool Equals(ToolData other)
    {
        return Equals(toolStrength, other.toolStrength) && tileDamagePerSecond.Equals(other.tileDamagePerSecond);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ToolData) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(toolStrength, tileDamagePerSecond);
    }

    public static bool operator ==(ToolData left, ToolData right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ToolData left, ToolData right)
    {
        return !Equals(left, right);
    }
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
public class TileInfo
{
    [HideInInspector] public string inspectorName;
    [HideInInspector] public string inspectorNameSimple;
    
    public TileBase tile;
    public float life;
    public ToolStrength requiredToBreak = ToolStrength.DEFAULT;
    public ItemStack lootOnBreak;
    
    public bool HasAnyLoot()
    {
        return ItemStack.IsValid(lootOnBreak);
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
    
    public void OnValidate(DatabaseSO database)
    {
        requiredToBreak.OnValidate(database);
        lootOnBreak.OnValidate(database);
        
        if (tile)
        {
            inspectorName = tile.name;
            if (requiredToBreak != ToolStrength.DEFAULT)
            {
                inspectorName += " (BREAK COND. [" + requiredToBreak.inspectorName + "])";
            }
            if (ItemStack.IsValidEditor(lootOnBreak, database))
            {
                inspectorName += " (LOOT [" + lootOnBreak.inspectorName + "])";
            }
        }
        else
        {
            inspectorName = "<invalid>";
        }
    }

    protected bool Equals(TileInfo other)
    {
        return Equals(tile, other.tile) && life.Equals(other.life) && Equals(requiredToBreak, other.requiredToBreak) && Equals(lootOnBreak, other.lootOnBreak);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TileInfo) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(tile, life, requiredToBreak, lootOnBreak);
    }

    public static bool operator ==(TileInfo left, TileInfo right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(TileInfo left, TileInfo right)
    {
        return !Equals(left, right);
    }
}

[Serializable]
public class Item
{
    [HideInInspector] public string inspectorName;
    
    public string id;
    public string name;
    public string description;
    public Sprite sprite;
    public TileInfo tileInfo = TileInfo.DEFAULT;
    public ToolData toolData = ToolData.DEFAULT;
    
    public void OnValidate(DatabaseSO database)
    {
        tileInfo.OnValidate(database);
        toolData.OnValidate(database);
        
        if (id != string.Empty)
        {
            inspectorName = id;
            if (tileInfo.tile)
            {
                // inspectorName += " (TILE [" + tileInfo.inspectorName + "])";
                if (ItemStack.IsValidEditor(tileInfo.lootOnBreak, database))
                {
                    inspectorName += " (TILE)";
                }
                else
                {
                    inspectorName += " (TILE NO LOOT)";
                }
            }
            if (toolData != ToolData.DEFAULT)
            {
                // inspectorName += " (TOOL [" + toolData.inspectorName + "])";
                inspectorName += " (TOOL)";
            }
        }
        else
        {
            inspectorName = "<invalid>";
        }
    }
}
    
[Flags]
public enum CraftingFlags
{
    CraftingTable   = 1 << 0,
    Furnace         = 1 << 1,
    // ...
}

[Serializable]
public class CraftRecipe
{
    [HideInInspector] public string inspectorName;
    
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

    public void OnValidate(DatabaseSO database)
    {
        itemCrafted.OnValidate(database);
        itemsRequired.ForEach(i => i.OnValidate(database));

        inspectorName = itemCrafted.inspectorName;
    }
}
