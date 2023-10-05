using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class StrateGeneration : MonoBehaviour
{

    [Header("Initialisation")]
    public PlayerInput Player;
    public int dimensionsX;
    private Vector2Int OffsetXY;
    public Tilemap tileGround;
    [SerializeField] private List<Strate> _strates = new List<Strate>();
    public TileBase tileBase;
    public SpriteRenderer shadow;
    int sizeY = 0;

    [Header("Filon")]
    public List<TilePos> LTiles = new List<TilePos>();
    public List<TilePos> LtilesVoisin = new List<TilePos>();
    public float ProbVoisin;
    public float ProbVoisinVoisin;

    [Header("Shader Graph")]
    [HideInInspector] public Texture2D wordTilesMap;
    [HideInInspector] public Texture2D PlayerTexture;
    public Material lightShader;

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
    [SerializeField] private CheckDecor _checkDecor;
    private List<Vector3Int> _posDecor = new List<Vector3Int>();
    void Awake()
    {
        
        InitializeGrid();
        if(_pasteGrotto != null)
            _pasteGrotto.SpawnGrotte();

        // Mining.OnMineTile += CallEventShadow;
        // Placing.OnPlaceTile += CallEventShadow;
    }

    private void CallEventShadow(Item item, Vector3Int cellPos, Vector2 vector)
    {
        // UpdateShadowGround();
    }

    private void Update()
    {
        // UpdatePlayerLight();
    }

    public void InitializeGrid()
    {
        OffsetXY.x = dimensionsX / 2;
        OffsetXY.y = 5;
        int debutStrate = 0;
        LTiles.Clear();
        LtilesVoisin.Clear();
        tileGround.ClearAllTiles();

        for ( int s = 0 ; s < _strates.Count(); s++) 
        {
            if(s > 1) 
            {
                debutStrate += _strates[s - 1].SizeY ;
            }
            if(s == 0)
            {
                FirstStrate(_strates[s]);
                continue;
            }
            
            if(_strates[s].transition) 
            {
                Transition(debutStrate, _strates[s]);
                continue;
            }
            for (int y = debutStrate; y < debutStrate+_strates[s].SizeY; y++)
            {
                for (int x = 0; x < dimensionsX; x++)
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), GetTileFromStrate(_strates[s], x, y));
                }
            }
        }
        SetFilons();
        sizeY = tileGround.size.y;
        ResetShadowGround();
        ResetShadowPlayer();
        shadow.transform.position = new Vector3(0, -tileGround.CellToWorld(new Vector3Int(0, (sizeY / 2) + OffsetXY.y)).y + 0.17f);
        shadow.transform.localScale = new Vector3(tileGround.size.x, -tileGround.size.y, tileGround.size.z) * 0.32f;
        UpdateShadowGround();



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
        int rng = Random.Range(start, start + st.SizeY );
        for (int x = 0; x < dimensionsX; x++)
        {
            for(int y = start; y < start + st.SizeY; y++) 
            {
                if( y < rng) 
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x,-y - OffsetXY.y), st.Tiles[0].Tile);
                }
                else 
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), st.Tiles[1].Tile);
                }
            }
            rng += Random.Range(-1, 2);
            rng = Mathf.Min(rng, start + st.SizeY-1);
            rng = Mathf.Max(rng, start);
        }
    }

    public void FirstStrate(Strate st) 
    {
        int treeDist = 0;
        int rng = Random.Range(0, st.SizeY);
        for (int x = 0; x < dimensionsX; x++)
        {
            for (int y = 0; y <= rng; y++)
            {
                if (y == rng)
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, y - OffsetXY.y), st.Tiles[0].Tile);
                }
                else
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, y - OffsetXY.y), st.Tiles[1].Tile);
                }
            }
            if( treeDist > _treeMinDist &&  Random.Range(0f,1f) < _treeChance/100f )
            {
                PasteTree(new Vector3Int(x - OffsetXY.x, rng - OffsetXY.y+1, 0));
                treeDist = 0;
            }
            if (Random.Range(0f, 1f) < _decorsChance / 100f && _Decor != null) 
            {
                _Decor.SetTile(new Vector3Int(x - OffsetXY.x, rng - OffsetXY.y + 1, 0), _decors[Random.Range(0, _decors.Length)]);
                _posDecor.Add(new Vector3Int(x - OffsetXY.x, rng - OffsetXY.y + 1, 0));
            }
            rng += Random.Range(-1, 2)* Random.Range(0, 2);
            rng = Mathf.Min(rng, st.SizeY );
            rng = Mathf.Max(rng, 1);
            treeDist++;
            //tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), GetTileFromStrate(_strates[s], x, y));
        }

        if (_checkDecor != null)
            _checkDecor.SetList(_posDecor);
    }
    public TileBase GetTileFromStrate(Strate strate, int x, int y)
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
                return strate.Tiles[i].Tile;
            }
        }
        return null;
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
            if(Tp.tiles != null)
            RecFilon(Tp.tiles, Tp.intX, Tp.intY, ProbVoisinVoisin);
        }
    }

    public TilePos[] RecFilon(TileBase T , int X , int Y , float Nb)
    {
        TilePos[] Ltemp =new TilePos[5];
        int n= 0;
        for ( int i = -1; i< 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                float rng = Random.Range(0.0f, 100.0f);
                TileBase temp = tileGround.GetTile(new Vector3Int(X + i - OffsetXY.x, -Y + j - OffsetXY.y));
                if (temp != T && temp != null && rng < Nb && ( i == 0 || j == 0))
                {
                    tileGround.SetTile(new Vector3Int(X + i - OffsetXY.x, -Y + j - OffsetXY.y), T);
                    Ltemp[n] = new TilePos(T, X+i, Y+j);
                    n++;
                }
            }
        }
        return Ltemp;
    }


    public void ResetShadowGround()
    {
        wordTilesMap = new Texture2D(dimensionsX, sizeY);
        wordTilesMap.filterMode = FilterMode.Point;
        lightShader.SetTexture("_ShadowTexture", wordTilesMap);
        for (int i = 0; i < dimensionsX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                wordTilesMap.SetPixel(i, j, new Color(1, 0, 0, 1));
            }
        }

        wordTilesMap.Apply();
    }
    public void ResetShadowPlayer()
    {
        PlayerTexture = new Texture2D(dimensionsX, sizeY);
        PlayerTexture.filterMode = FilterMode.Point;
        lightShader.SetTexture("_PlayerLight", PlayerTexture);
        for (int i = 0; i < dimensionsX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                PlayerTexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }



    public void UpdatePlayerLight()
    {
        ResetShadowPlayer();
        Vector3Int Cell = tileGround.WorldToCell(Player.transform.position);
        for (int i = -10; i < 11; i++)
        {
            for (int j = -10; j < 11; j++)
            {
                PlayerTexture.SetPixel(Cell.x + OffsetXY.x + i, sizeY - Cell.y + j - OffsetXY.y, new Color(1 - (0.1f*Mathf.Abs(i)+0.1f* Mathf.Abs(j)), 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }


    public void UpdateShadowGround()
    {
        ResetShadowGround();
        for (int i = 0; i < dimensionsX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                float temp = 1;

                if (tileGround.GetTile(new Vector3Int(i - OffsetXY.x, -j - OffsetXY.y)) != null)
                {
                    temp = wordTilesMap.GetPixel(i, j - 1).r - 0.2f;

                } 
                else
                {
                    temp = 1;
                }

                
                wordTilesMap.SetPixel(i, j, new Color(temp, 0, 0, 1));

            }
        }
        for (int i = 0; i < dimensionsX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (tileGround.GetTile(new Vector3Int(i - OffsetXY.x + 1, -j - OffsetXY.y)) == null
                    || tileGround.GetTile(new Vector3Int(i - OffsetXY.x - 1, -j - OffsetXY.y)) == null)
                {
                    wordTilesMap.SetPixel(i, j, new Color(wordTilesMap.GetPixel(i, j).r+0.3f, 0, 0, 1));
                }
            }
        }

                wordTilesMap.Apply();
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

