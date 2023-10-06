using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Utilities;

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
    public Vector3 tileSize = new Vector2(0.32f, 0.32f);
    public Vector3 entityCheckMargin = new Vector2(0.05f, 0.05f);
    public LayerMask entityLayers;
    [SerializeField] private PolygonCollider2D _confinerBox;
    public PolygonCollider2D ConfinerBox => _confinerBox;

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
    
    public List<Tilemap> GetTilemapsFromLayers(TilemapLayer layers)
    {
        List<Tilemap> tilemaps = new List<Tilemap>();
        
        if ((layers & TilemapLayer.Background) != 0)
        {
            tilemaps.Add(BackgroundTilemap);
        }
        if ((layers & TilemapLayer.Decor) != 0)
        {
            tilemaps.Add(DecorTilemap);
        }
        if ((layers & TilemapLayer.Ground) != 0)
        {
            tilemaps.Add(GroundTilemap);
        }
        
        return tilemaps;
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
        return UtilTilemap.CellToWorld(tilePos) + (tileSize / 2.0f);
    }
    
    private bool HasRequiredTile(ref List<Tilemap> tilemaps, Vector3Int inTilePos, Predicate<TileBase> predicate = null)
    {
        return tilemaps.Select(tilemap => tilemap.GetTile(inTilePos))
            .Any(tile => tile && (predicate == null || predicate(tile)) && GameManager.Instance.database.GetItemByTile(tile).tileInfo.canBeBuiltFrom);
    }
    
    private bool HasAllRequiredNeighbours(TileNeighbour neighbours, ref List<Tilemap> tilemaps, Vector3Int inCenterTilePos, Predicate<TileBase> predicate = null)
    {
        var tilePosUp = inCenterTilePos + Vector3Int.up;
        var tilePosDown = inCenterTilePos + Vector3Int.down;
        var tilePosLeft = inCenterTilePos + Vector3Int.left;
        var tilePosRight = inCenterTilePos + Vector3Int.right;
                    
        if ((neighbours & TileNeighbour.Up) != 0)
        {
            if (!HasRequiredTile(ref tilemaps, tilePosUp, predicate)) return false;
        }
        if ((neighbours & TileNeighbour.Down) != 0)
        {
            if (!HasRequiredTile(ref tilemaps, tilePosDown, predicate)) return false;
        }
        if ((neighbours & TileNeighbour.Left) != 0)
        {
            if (!HasRequiredTile(ref tilemaps, tilePosLeft, predicate)) return false;
        }
        if ((neighbours & TileNeighbour.Right) != 0)
        {
            if (!HasRequiredTile(ref tilemaps, tilePosRight, predicate)) return false;
        }

        return true;
    }

    private bool CanItemBePlaced(Item item, Vector2 worldPos, bool isRecheck)
    {
        if (item is null)
        {
            return false;
        }
        
        var placeTilemap = item.tileInfo.placingRules.GetPlaceTilemap();
        if (!item.tileInfo.tile || !placeTilemap)
        {
            return false;
        }
        
        var originTilePos = placeTilemap.WorldToCell(worldPos);
        
        var tilePos = originTilePos; // TODO size in a for loop maybe one day

        if (!isRecheck && placeTilemap == GroundTilemap)
        {
            var hit = Physics2D.BoxCast(GetWorldCenterOfTile(tilePos), tileSize + entityCheckMargin, 0.0f, Vector2.zero, Mathf.Infinity, entityLayers);
            if (hit.collider)
            {
                return false;
            }
        }

        bool isTileOccupied = false;
        if (!isRecheck)
        {
            if (placeTilemap == DecorTilemap || placeTilemap == GroundTilemap)
            {
                isTileOccupied = (DecorTilemap.GetTile(tilePos) != null) || (GroundTilemap.GetTile(tilePos) != null);
            }
            else
            {
                isTileOccupied = placeTilemap.GetTile(tilePos) != null;
            }
        }

        bool areRequirementsMet = item.tileInfo.placingRules.requirements.Count <= 0;
        foreach (TilePlacingRequirements requirement in item.tileInfo.placingRules.requirements)
        {
            if (!requirement.HasAnyRequirements())
            {
                Debug.LogWarning("ERROR: Forgot empty requirements inside the database (\"" + item.id + "\"");
                continue;
            }

            if (requirement.HasAnyCenterRequirements())
            {
                if (requirement.whitelistCenter.Count > 0)
                {
                    if (!HasRequiredTile(ref _gameplayTilemaps, tilePos,
                            tile => requirement.whitelistCenter.Contains(GameManager.Instance.database.GetItemByTile(tile).id))) continue;
                }
                else
                {
                    var tilemaps = GetTilemapsFromLayers(requirement.whitelistCenterLayers);
                    if (!HasRequiredTile(ref tilemaps, tilePos)) continue;
                }
            }

            if (!(isRecheck && item.tileInfo.canBeBuiltFrom)) // we don't care about rechecking neighbour requirements if we are a canBeBuiltFrom tile
            {
                if (requirement.HasAnyNeighbourRequirements())
                {
                    if (requirement.whitelistNeighbours.Count > 0)
                    {
                        if (!HasAllRequiredNeighbours(requirement.requiredNeighbours, ref _gameplayTilemaps, tilePos,
                                tile => requirement.whitelistNeighbours.Contains(GameManager.Instance.database.GetItemByTile(tile).id))) continue;
                    }
                    else if (requirement.whitelistNeighbourLayers != 0)
                    {
                        var tilemaps = GetTilemapsFromLayers(requirement.whitelistNeighbourLayers);
                        if (!HasAllRequiredNeighbours(requirement.requiredNeighbours, ref tilemaps, tilePos)) continue;
                    }
                    else
                    {
                        // any neighbour
                        if (!HasAllRequiredNeighbours(requirement.requiredNeighbours, ref _gameplayTilemaps, tilePos)) continue;
                    }
                }
            }
            
            areRequirementsMet = true;
            break;
        }
        
        return !isTileOccupied && areRequirementsMet;
    }

    public (Tilemap, TileBase, Vector3Int) FindHitTile(Vector2 worldPos, Item heldItem)
    {
        var cellPos = UtilTilemap.WorldToCell(worldPos);
        
        Tilemap tilemap = null;
        
        if (GroundTilemap.GetTile(cellPos) != null)
        {
            tilemap = GroundTilemap;
        }
        else if (DecorTilemap.GetTile(cellPos) != null)
        {
            tilemap = DecorTilemap;
        }
        else if (BackgroundTilemap.GetTile(cellPos) != null)
        {
            if (heldItem != null && heldItem.toolData.isTool &&
                ((heldItem.toolData.toolStrength.type & ToolType.Hammer) != 0))
            {
                tilemap = BackgroundTilemap;
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
