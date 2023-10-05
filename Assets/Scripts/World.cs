using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class World : Singleton<World>
{
    [Flags]
    public enum TilemapLayer
    {
        Background = 1 << 0,
        Decor = 1 << 1,
        Ground = 1 << 2,
    }

    [field: Header("Tilemaps")]
    [field: SerializeField] public Tilemap BackgroundTilemap { get; private set; }
    [field: SerializeField] public Tilemap DecorTilemap { get; private set; }
    [field: SerializeField] public Tilemap GroundTilemap { get; private set; }
    
    [field: Header("Visualization")]
    [field: SerializeField] public Tilemap VisualizationTilemap { get; private set; }
    public Color vizGoodColor;
    public Color vizBadColor;

    public Tilemap UtilTilemap => GroundTilemap;
    
    [field: Header("Prefabs")]
    [field: SerializeField] public GameObject DropPrefab { get; private set; }
    
    [field: Header("Variables")]
    public float tileVerificationDelay = 0.1f;

    public static event Action<Tilemap, Vector3Int, Item> OnMineTile; // tilemap, tilePos, item
    public static event Action<Tilemap, Vector3Int, Item> OnPlaceTile; // tilemap, tilePos, item

    private List<Tilemap> _gameplayTilemaps = new List<Tilemap>();

    protected override void SingletonAwake()
    {
        _gameplayTilemaps.Add(BackgroundTilemap);
        _gameplayTilemaps.Add(DecorTilemap);
        _gameplayTilemaps.Add(GroundTilemap);
    }

    public bool IsAWorldTilemap(Tilemap tilemap)
    {
        return tilemap == BackgroundTilemap
               || tilemap == DecorTilemap
               || tilemap == GroundTilemap;
    }

    private void RecheckTile(Vector3Int tilePos, ref List<(Tilemap, Vector3Int)> tilesToDestroy)
    {
        foreach (var tilemap in _gameplayTilemaps)
        {
            TileBase tile = tilemap.GetTile(tilePos);
            if (tile != null && !CanItemBePlaced(GameManager.Instance.database.GetItemByTile(tile), GetWorldCenterOfTile(tilePos), true))
            {
                tilesToDestroy.Add((tilemap, tilePos));
            }
        }
    }
    
    private IEnumerator RecheckTileNeighbours(Vector3Int tilePos)
    {
        yield return new WaitForSeconds(tileVerificationDelay);
        
        List<(Tilemap, Vector3Int)> tilesToDestroy = new List<(Tilemap, Vector3Int)>();
        
        // Recheck SELF and NEIGHBOURS (center, up, down, left, right)
        RecheckTile(tilePos, ref tilesToDestroy);
        RecheckTile(tilePos + Vector3Int.up, ref tilesToDestroy);
        RecheckTile(tilePos + Vector3Int.down, ref tilesToDestroy);
        RecheckTile(tilePos + Vector3Int.left, ref tilesToDestroy);
        RecheckTile(tilePos + Vector3Int.right, ref tilesToDestroy);
        
        tilesToDestroy.ForEach(t => TryDestroyTile(t.Item1, t.Item2));
    }
    
    public bool TryDestroyTile(Tilemap tilemap, Vector3Int tilePos)
    {
        if (!IsAWorldTilemap(tilemap))
        {
            return false;
        }
        
        Item item = GameManager.Instance.database.GetItemByTile(tilemap.GetTile(tilePos));
        if (item is null)
        {
            Debug.LogWarning("Unreferenced tile in database!");
            return false;
        }

        if (!tilemap.GetTile(tilePos))
        {
            return false;
        }
        
        tilemap.SetTile(tilePos, null);
        StartCoroutine(RecheckTileNeighbours(tilePos));
        OnMineTile?.Invoke(tilemap, tilePos, item);

        if (!item.tileInfo.breakingRules.HasAnyLoot())
        {
            return true; // tile does not drop anything
        }
        
        GenerateDrop(item.tileInfo.breakingRules.GenerateLoot(), GetWorldCenterOfTile(tilePos)).AddRandomForce();
        return true;
    }
    
    public bool TryPlaceTile(string itemID, Vector2 worldPos)
    {
        Item item = GameManager.Instance.database.GetItemById(itemID);
        if (!CanItemBePlaced(item, worldPos, false))
        {
            return false;
        }

        var tilemap = item.tileInfo.placingRules.GetPlaceTilemap();
        var tilePos = tilemap.WorldToCell(worldPos);
        
        tilemap.SetTile(tilePos, item.tileInfo.tile);
        OnPlaceTile?.Invoke(tilemap, tilePos, item);
        
        return true;
    }
    
    public Drop GenerateDrop(ItemStack itemStack, Vector2 worldPos)
    {
        var drop = Instantiate(DropPrefab, worldPos, Quaternion.identity).GetComponent<Drop>();
        drop.ItemStack = itemStack;
        return drop;
    }
    
    public Vector2 GetWorldCenterOfTile(Vector3Int tilePos)
    {
        return UtilTilemap.CellToWorld(tilePos) + new Vector3(0.16f, 0.16f, 0.0f);
    }
    
    private bool CanItemBePlaced(Item item, Vector2 worldPos, bool skipSelf)
    {
        if (item is null)
        {
            return false;
        }
        
        var tilemap = item.tileInfo.placingRules.GetPlaceTilemap();
        if (!item.tileInfo.tile || !tilemap)
        {
            return false;
        }
        
        var hit = Physics2D.Raycast(worldPos, Vector2.zero, Mathf.Infinity);
        if (hit.collider != null && hit.collider.gameObject.tag.Equals("Player")) // TODO polish
        {
            return false;
        }
        
        var tilePos = tilemap.WorldToCell(worldPos);

        bool isTileOccupied = false;
        if (!skipSelf)
        {
            if (tilemap == DecorTilemap || tilemap == GroundTilemap)
            {
                isTileOccupied = (DecorTilemap.GetTile(tilePos) != null) || (GroundTilemap.GetTile(tilePos) != null);
            }
            else
            {
                isTileOccupied = tilemap.GetTile(tilePos) != null;
            }
        }

        return !isTileOccupied;
        
        // if (World.Instance.GroundTilemap.GetTile(cellPos) is not null)
        // {
        //     return false;
        // }
        //
        // if (World.Instance.BackgroundTilemap.GetTile(cellPos) is not null)
        // {
        //     return true;
        // }
        //
        // var cellAbove = cellPos + new Vector3Int(0, 1);
        // var cellBelow = cellPos + new Vector3Int(0, -1);
        // var cellLeft = cellPos + new Vector3Int(-1, 0);
        // var cellRight = cellPos + new Vector3Int(1, 0);
        //
        // if (World.Instance.GroundTilemap.GetTile(cellBelow) is not null ||
        //     World.Instance.GroundTilemap.GetTile(cellLeft) is not null ||
        //     World.Instance.GroundTilemap.GetTile(cellRight) is not null ||
        //     World.Instance.GroundTilemap.GetTile(cellAbove) is not null)
        // {
        //     // Debug.Log($" Below:{World.Instance.GroundTilemap.GetTile(cellBelow) is not null}" +
        //     //           $"Left: {World.Instance.GroundTilemap.GetTile(cellLeft) is not null}" +
        //     //           $"Right: {World.Instance.GroundTilemap.GetTile(cellRight) is not null}" +
        //     //           $"Above: {World.Instance.GroundTilemap.GetTile(cellAbove) is not null}");
        //     return true;
        // }
        //
        // return false;
    }

    public (Tilemap, TileBase, Vector3Int) FindHitTile(Vector2 worldPos, Item heldItem)
    {
        var cellPos = UtilTilemap.WorldToCell(worldPos);
        
        Tilemap tilemap = null;
        if (heldItem != null && heldItem.toolData.isTool && heldItem.toolData.toolStrength.type == ToolType.Hammer)
        {
            if (BackgroundTilemap.GetTile(cellPos) != null)
            {
                tilemap = BackgroundTilemap;
            }
        }
        else
        {
            if (GroundTilemap.GetTile(cellPos) != null)
            {
                tilemap = GroundTilemap;
            }
            else if (DecorTilemap.GetTile(cellPos) != null)
            {
                tilemap = DecorTilemap;
            }
        }

        if (tilemap == null)
        {
            return (null, null, Vector3Int.zero);
        }

        return (tilemap, tilemap.GetTile(cellPos), cellPos);
    }

    private Vector3Int _lastVizTilePos;

    public void VisualizeHeldItem(Vector2 worldPos, Item heldItem, bool playerCondMet = true)
    {
        VisualizationTilemap.SetTile(_lastVizTilePos, null);
        
        if (heldItem == null)
        {
            return;
        }
        
        if (!heldItem.tileInfo.tile || !heldItem.tileInfo.placingRules.GetPlaceTilemap())
        {
            return;
        }
        
        var tilePos = VisualizationTilemap.WorldToCell(worldPos);
        _lastVizTilePos = tilePos;
        
        VisualizationTilemap.SetTile(tilePos, heldItem.tileInfo.tile);

        if (CanItemBePlaced(heldItem, worldPos, false) && playerCondMet)
        {
            VisualizationTilemap.color = vizGoodColor;
        }
        else
        {
            VisualizationTilemap.color = vizBadColor;
        }
    }
}
