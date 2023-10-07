using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class StrateGeneration : MonoBehaviour
{
    [Header("Initialisation")]
    public int dimensionsX;
    [HideInInspector] public Vector2Int OffsetXY;
    public Tilemap tileGround;
    [SerializeField] private List<Strate> _strates = new List<Strate>();
    private Vector2Int size;

    [Header("Filon")]
    [HideInInspector] public List<TilePos> LTiles = new List<TilePos>();
    [HideInInspector] public List<TilePos> LtilesVoisin = new List<TilePos>();
    public float ProbVoisin;
    public float ProbVoisinVoisin;

    public FogOfWarGenerator Fog;

    [Header("Auto Update Generator")]
    public bool autoUpdate;
    [SerializeField] private PasteGrotte _pasteGrotto;

    [Header("Tree")]
    [SerializeField] private Tilemap[] _trees;
    [SerializeField] private Tilemap _tileMapTree;
    [SerializeField] private int _treeMinDist;
    [SerializeField] private int _treeChance;

    [Header("Decors")]
    [SerializeField] private Tilemap _Decor;
    [SerializeField] private TileBase[] _decors;
    [SerializeField] private int _decorsChance;
    private List<Vector3Int> _posDecor = new List<Vector3Int>();

    [Header("Bg")]
    [SerializeField] private Tilemap _bg;
    [SerializeField] private TileBase _bgTile;

    [Header("Artifact")]
    [SerializeField] private Tilemap _artifact;
    [SerializeField] private CraftingTableTile _artifactChest;
    [SerializeField] private TileBase _artifactTile;
    private int _artefactRange;
    private Vector3Int _artefactPos;

    void Start()
    {
        _artefactRange = Mathf.Abs(_artifact.cellBounds.min.x - _artifact.cellBounds.max.x);
        InitializeGrid();
        if (_pasteGrotto != null)
            _pasteGrotto.SpawnGrotte();
    }
    public List<Strate> GetStrates() { return _strates; }
    public TileBase GetTile(int i, int j)
    {
        return tileGround.GetTile(new Vector3Int(i - tileGround.size.x/2, j - tileGround.size.y +1 + OffsetXY.y));
    }

    public Vector2Int GetTilesPos(Vector3 Player)
    {
        return (Vector2Int)tileGround.WorldToCell(Player);
    }


    public void InitializeGrid()
    {
        OffsetXY.x = dimensionsX / 2;
        OffsetXY.y = _strates[0].SizeY;
        int debutStrate = 0;
        LTiles.Clear();
        LtilesVoisin.Clear();
        tileGround.ClearAllTiles();

        for (int s = 0; s < _strates.Count(); s++)
        {
            if (s > 1)
            {
                debutStrate += _strates[s - 1].SizeY;
            }
            if (s == 0)
            {
                FirstStrate(_strates[s]);
                continue;
            }

            if (_strates[s].transition)
            {
                Transition(debutStrate, _strates[s]);
                continue;
            }
            for (int y = debutStrate; y < debutStrate + _strates[s].SizeY; y++)
            {
                for (int x = 0; x < dimensionsX; x++)
                {
                    TilesStrate ts = GetTileFromStrate(_strates[s], x, y);
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y ), ts.Tile);
                    if(_bg != null)
                        _bg.SetTile(new Vector3Int(x - OffsetXY.x, -y ), ts.TilesBG);
                }
            }
        }
        SetFilons();
        size = (Vector2Int)tileGround.size;

        float pair = -0.18f;
        if (tileGround.size.y % 2 == 0) pair = 0;
        Vector3Int middelY = new Vector3Int(0,(tileGround.size.y/2)-1 );
        Vector2 shadowPos = new Vector3(0, -tileGround.CellToWorld(middelY).y + pair + OffsetXY.y * 0.32f);
        Vector2 ShadowScale = new Vector3(tileGround.size.x, tileGround.size.y , tileGround.size.z) * 0.32f;

        if (Fog != null)
            Fog.InitShadow(size, shadowPos, ShadowScale);



    }

    public void PasteArtifact(Vector3Int pos)
    {
        Tilemap toPastePrefab = _artifact;
        _artefactPos = pos;
        for (int i = toPastePrefab.cellBounds.min.x - 1; i < toPastePrefab.cellBounds.max.x + 1; i++)
        {
            for (int j = toPastePrefab.cellBounds.min.y - 1; j < toPastePrefab.cellBounds.max.y + 1; j++)
            {
                TileBase tile = toPastePrefab.GetTile(new Vector3Int(i, j));
                
                if (tile != null)
                {
                    Tilemap placeTilemap;
                    if (GameManager.Instance.database.GetItemByTile(tile) == null) 
                    {
                        placeTilemap = _tileMapTree;
                        placeTilemap.SetTile(new Vector3Int(i, j) + pos, _artifactChest);
                        _artefactPos = new Vector3Int(i, j) + pos;
                        StartCoroutine(PlaceArtifact(_artefactPos));
                    }  
                    else 
                    {
                        placeTilemap = World.Instance.GetTilemapsFromLayers(GameManager.Instance.database.GetItemByTile(tile).tileInfo.placingRules.placeLayer)[0];
                        placeTilemap.SetTile(new Vector3Int(i, j) + pos, tile);
                    }

                   
                }

            }
        }
    }
    public IEnumerator PlaceArtifact(Vector3Int posChest)
    {
        yield return new WaitForSeconds(0.5f);
        Tilemap placeTilemap = _tileMapTree;
        Vector3 pos = placeTilemap.CellToWorld(posChest);
        pos -= Camera.main.transform.forward * 10 - Camera.main.transform.up * 0.16f;
        Debug.DrawRay(pos, Camera.main.transform.forward * 11, Color.white, 1000);
        RaycastHit2D hit = Physics2D.Raycast(pos, Camera.main.transform.forward, 11);

        List<ItemStack> list = new List<ItemStack>();
        for (int i = 0; i < InventorySystem.Instance._nbChestSlots; i++)
        {
            list.Add(null);
        }
        int nbrOfitem = Random.Range(1, InventorySystem.Instance._nbChestSlots);
        ItemStack itemStack = new ItemStack();
        var item = GameManager.Instance.database.GetItemByTile(_artifactTile);
        itemStack.itemID = item.id;
        itemStack.number =1;
        list[0] = itemStack;

        hit.collider.GetComponent<Chest>().PlaceLoot(list);
    }
    public void PasteTree(Vector3Int pos)
    {
        if (_trees == null || _trees.Length == 0 || _tileMapTree == null)
            return;
        Tilemap toPastePrefab = _trees[Random.Range(0, _trees.Length)];
        for (int i = toPastePrefab.cellBounds.min.x - 1; i < toPastePrefab.cellBounds.max.x + 1; i++)
        {
            for (int j = toPastePrefab.cellBounds.min.y - 1; j < toPastePrefab.cellBounds.max.y + 1; j++)
            {
                TileBase tile = toPastePrefab.GetTile(new Vector3Int(i, j));
                if (tile != null)
                {
                    _tileMapTree.SetTile(new Vector3Int(i, j) + pos, tile);
                }

            }
        }
    }

    public void Transition(int start, Strate st)
    {
        int rng = Random.Range(start, start + st.SizeY);
        for (int x = 0; x < dimensionsX; x++)
        {
            for (int y = start; y < start + st.SizeY; y++)
            {
                if (y < rng)
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x,-y ), st.Tiles[0].Tile);
                    if (_bg != null)
                        _bg.SetTile(new Vector3Int(x - OffsetXY.x, -y ), st.Tiles[0].TilesBG);
                }
                else
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y ), st.Tiles[1].Tile);
                    if (_bg != null)
                        _bg.SetTile(new Vector3Int(x - OffsetXY.x, -y ), st.Tiles[1].TilesBG);
                }
            }
            rng += Random.Range(-1, 2);
            rng = Mathf.Min(rng, start + st.SizeY - 1);
            rng = Mathf.Max(rng, start);
        }
    }

    public void FirstStrate(Strate st)
    {
        int treeDist = 0;
        int rng = Random.Range(0, st.SizeY);
        bool left = Random.Range(0,2) == 1;
        int artefactPosX;
        if (left) 
        {
            artefactPosX = Random.Range(_artefactRange, dimensionsX/2 - _artefactRange*2);
        }
        else
            artefactPosX = Random.Range(dimensionsX / 2 + _artefactRange * 2, dimensionsX - _artefactRange);


        for (int x = 0; x < dimensionsX; x++)
        {
            int diff = Mathf.Abs( x- artefactPosX);
            for (int y = 0; y <= rng; y++)
            {
                if (tileGround.GetTile(new Vector3Int(x - OffsetXY.x, y)) != null)
                    continue;
                if (y == rng)
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, y ), st.Tiles[0].Tile);

                }
                else
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, y ), st.Tiles[1].Tile);
                }
            }
            if(artefactPosX == x) 
            {
                PasteArtifact(new Vector3Int(x - OffsetXY.x, rng + 1, 0));
            }
            if (diff > _artefactRange  && treeDist > _treeMinDist && Random.Range(0f, 1f) < _treeChance / 100f)
            {
                PasteTree(new Vector3Int(x - OffsetXY.x, rng  + 1, 0));
                treeDist = 0;
            }
            if (diff > _artefactRange && Random.Range(0f, 1f) < _decorsChance / 100f && _Decor != null && treeDist > 1)
            {
                _Decor.SetTile(new Vector3Int(x - OffsetXY.x, rng  + 1, 0), _decors[Random.Range(0, _decors.Length)]);
                _posDecor.Add(new Vector3Int(x - OffsetXY.x, rng  + 1, 0));
            }
            rng += Random.Range(-1, 2) * Random.Range(0, 2);
            rng = Mathf.Min(rng, st.SizeY);
            rng = Mathf.Max(rng, 1);
            treeDist++;
            //tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y ), GetTileFromStrate(_strates[s], x, y));
        }
    }
    public TilesStrate GetTileFromStrate(Strate strate, int x, int y)
    {
        for (int i = 0; i < strate.Tiles.Count(); i++)
        {
            float pourcentage = strate.Tiles[i].Pourcentage;
            float rng = Random.Range(0.0f, 100.0f);
            if ((rng < pourcentage) || (i == strate.Tiles.Count() - 1))
            {
                if (strate.Tiles[i].IsMinerai)
                {
                    LTiles.Add(new TilePos(strate.Tiles[i].Tile, x, y));
                }
                return strate.Tiles[i];
            }
        }
        return strate.Tiles[0];
    }

    public void SetFilons()
    {
        foreach (TilePos item in LTiles)
        {
            foreach (TilePos TP in RecFilon(item.tiles, item.intX, item.intY, ProbVoisin))
            {
                LtilesVoisin.Add(TP);
            }
        }
        foreach (TilePos Tp in LtilesVoisin)
        {
            if (Tp.tiles != null)
                RecFilon(Tp.tiles, Tp.intX, Tp.intY, ProbVoisinVoisin);
        }
    }

    public TilePos[] RecFilon(TileBase T, int X, int Y, float Nb)
    {
        TilePos[] Ltemp = new TilePos[5];
        int n = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                float rng = Random.Range(0.0f, 100.0f);
                TileBase temp = tileGround.GetTile(new Vector3Int(X + i - OffsetXY.x, -Y + j ));
                if (temp != T && temp != null && rng < Nb && (i == 0 || j == 0))
                {
                    tileGround.SetTile(new Vector3Int(X + i - OffsetXY.x, -Y + j ), T);
                    Ltemp[n] = new TilePos(T, X + i, Y + j);
                    n++;
                }
            }
        }
        return Ltemp;
    }
}
[Serializable]
public struct Strate
{
    public TilesStrate[] Tiles;
    public int SizeY;
    public bool transition;
}

[Serializable]
public struct TilesStrate
{
    public TileBase Tile;
    public TileBase TilesBG;
    public float Pourcentage;
    public bool IsMinerai;
}

[Serializable]
public struct TilePos
{
    public TileBase tiles;
    public int intX;
    public int intY;

    public TilePos(TileBase t, int x, int y)
    {
        tiles = t;
        intX = x;
        intY = y;
    }
}

