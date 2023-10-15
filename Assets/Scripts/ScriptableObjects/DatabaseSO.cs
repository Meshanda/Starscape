using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;
// ReSharper disable NonReadonlyMemberInGetHashCode

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
    public ItemStack GetLootByTile(TileBase tile)
    {
        return items.FirstOrDefault(item => item.tileInfo.tile && item.tileInfo.tile.Equals(tile)).tileInfo.breakingRules.loot.Clone();
    }

    #region Inspector
    
    private void OnValidate()
    {
        List<string> IDs = new();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == "reset")
            {
                items[i] = new Item();
            }
            
            var item = items[i];

            item.inspectorName = string.Empty;
            
            if (item.id == string.Empty || IDs.Contains(item.id))
            {
                item.inspectorName = "<invalid> ";
                continue;
            }

            IDs.Add(item.id);
        }
        
        items.ForEach(i => i.OnValidate(this));
        craftRecipes.ForEach(r => r.OnValidate(this));
    }
    
    #endregion
}

[Serializable]
public class ToolStrength
{
    #region Inspector
    [HideInInspector] public string inspectorName;
    #endregion
    
    // public ToolEffect effect; // silk touch...
    public ToolType type;
    public int level;
    
    public bool IsEnoughFor(ToolStrength strength)
    {
        return level >= strength.level && (((type & strength.type) != 0) || strength.type == 0);
    }
    
    #region Inspector
    
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
    
    #endregion
}

[Serializable]
public class ToolData
{
    public bool isTool = false; // if not a tool, the item is considered to be as strong as an empty hand
    public ToolStrength toolStrength = new();
    public float tileDamagePerSecond = 1.0f;
    // public float entityDamage; // TODO?
    // public float durability; // TODO?

    private static ToolData _DEFAULT;
    public static ToolData DEFAULT => _DEFAULT ??= new(); // only use at runtime
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
public class TileBreakingRules
{
    public float life = 1.0f;
    public ToolStrength requiredStrength = new();
    public ItemStack loot;
    
    public bool HasAnyLoot()
    {
        return ItemStack.IsValid(loot);
    }
    
    public ItemStack GenerateLoot()
    {
        return loot.Clone();
    }
}

[Flags]
public enum TileNeighbour
{
    Up      = 1 << 0,
    Down    = 1 << 1,
    Left    = 1 << 2,
    Right   = 1 << 3
}

[Serializable]
public class TilePlacingRequirements
{
    #region Inspector
    [HideInInspector] public string inspectorName;
    #endregion
    
    [Header("Center Tile (requiredCenter > requiredCenterLayers)")]
    public List<string> whitelistCenter = new List<string>(); // on the placing tile, we need to find at least one that is in this array
    public World.TilemapLayer whitelistCenterLayers; // on the placing tile, we need to find at least one that is in this var
    
    [Header("Neighbour Tiles IF requiredNeighbours (whitelistNeighbours > whitelistNeighbourLayers)")]
    public TileNeighbour requiredNeighbours; // do we need specific neighbours in order to be placed?
    public List<string> whitelistNeighbours = new List<string>(); // ^^^ in all directions, we need to find at least one that is in this array
    public World.TilemapLayer whitelistNeighbourLayers; // ^^^ in all directions, we need to find at least one that is in this var

    public TilePlacingRequirements(TileNeighbour requiredNeighbours = default, World.TilemapLayer whitelistNeighbourLayers = default)
    {
        this.requiredNeighbours = requiredNeighbours;
        this.whitelistNeighbourLayers = whitelistNeighbourLayers;
    }

    public bool HasAnyRequirements()
    {
        return HasAnyCenterRequirements() || HasAnyNeighbourRequirements();
    }
    
    public bool HasAnyCenterRequirements()
    {
        return whitelistCenter.Count > 0 || whitelistCenterLayers != 0;
    }
    
    public bool HasAnyNeighbourRequirements()
    {
        return requiredNeighbours != 0;
    }

    #region Inspector
    
    public void OnValidate(DatabaseSO database)
    {
        if (!HasAnyRequirements())
        {
            inspectorName = "<empty>";
        }
        else
        {
            inspectorName = string.Empty;
            if (HasAnyCenterRequirements())
            {
                if (whitelistCenter.Count > 0)
                {
                    inspectorName += " center(<specific>)";
                }
                else
                {
                    inspectorName += " center(<" + whitelistCenterLayers + ">)";
                }
            }
            if (HasAnyNeighbourRequirements())
            {
                inspectorName += " neighbours(" + requiredNeighbours;
                if (whitelistNeighbours.Count > 0)
                {
                    inspectorName += " <specific>)";
                }
                else if (whitelistNeighbourLayers != 0)
                {
                    inspectorName += " <" + whitelistNeighbourLayers + ">)";
                }
                else
                {
                    inspectorName += ")";
                }
            }
        }
    }
    
    #endregion
}

[Serializable]
public class TilePlacingRules
{
    // public Vector2 tileSize = Vector2.one; // TODO size maybe one day
    public World.TilemapLayer placeLayer = World.TilemapLayer.Ground;
    public List<TilePlacingRequirements> requirements = new List<TilePlacingRequirements>() // if any are valid, we can place.
    {
        new(TileNeighbour.Up, World.TilemapLayer.Ground),
        new(TileNeighbour.Down, World.TilemapLayer.Ground),
        new(TileNeighbour.Left, World.TilemapLayer.Ground),
        new(TileNeighbour.Right, World.TilemapLayer.Ground),
    };

    [CanBeNull]
    public Tilemap GetPlaceTilemap()
    {
        var tilemaps = World.Instance.GetTilemapsFromLayers(placeLayer);
        return tilemaps.Count > 0 ? tilemaps[0] : null;
    }

    #region Inspector
    
    public void OnValidate(DatabaseSO database)
    {
        requirements.ForEach(r => r.OnValidate(database));
    }

    #endregion
}

[Serializable]
public class TileInfo
{
    public TileBase tile;
    public bool canBeBuiltFrom = true;
    public TilePlacingRules placingRules = new();
    public TileBreakingRules breakingRules = new();

    #region Inspector

    public void OnValidate(DatabaseSO database)
    {
        placingRules.OnValidate(database);
    }

    #endregion
}

[Serializable]
public class Item
{
    #region Inspector
    [HideInInspector] public string inspectorName;
    #endregion
    
    public string id = string.Empty;
    public string name;
    public string description;
    public Sprite sprite;
    public TileInfo tileInfo = new();
    public ToolData toolData = new();
    public UsableItem.UsableItemType useType = UsableItem.UsableItemType.None;

    #region Inspector
    
    public void OnValidate(DatabaseSO database)
    {
        tileInfo.OnValidate(database);
        
        inspectorName += id;
        
        if (tileInfo.tile)
        {
            if (ItemStack.IsValidEditor(tileInfo.breakingRules.loot, database))
            {
                inspectorName += " (TILE)";
            }
            else
            {
                inspectorName += " (TILE NO LOOT)";
            }
        }
        
        if (toolData.isTool)
        {
            inspectorName += " (TOOL)";
        }
        
        if (useType != UsableItem.UsableItemType.None)
        {
            inspectorName += " (USE)";
        }
    }
    
    #endregion
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
    #region Inspector
    [HideInInspector] public string inspectorName;
    #endregion
    
    public List<ItemStack> itemsRequired = new List<ItemStack>();
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

    #region Inspector
    
    public void OnValidate(DatabaseSO database)
    {
        itemCrafted?.OnValidate(database);
        itemsRequired.ForEach(i => i?.OnValidate(database));

        inspectorName = itemCrafted?.inspectorName;
        if (itemsRequired.Count > 0)
        {
            inspectorName += " [ ";
            itemsRequired.ForEach(i => inspectorName += "<" + i?.inspectorName + "> ");
            inspectorName += "]";
        }
    }
    
    #endregion
}
