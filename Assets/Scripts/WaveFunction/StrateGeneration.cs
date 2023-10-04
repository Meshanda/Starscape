using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using Random = UnityEngine.Random;

public class StrateGeneration : MonoBehaviour
{
    [Header("Initialisation")]
    public int dimensionsX;
    public Vector2Int OffsetXY;
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

    void Start()
    {
        InitializeGrid();
        if(_pasteGrotto != null)
            _pasteGrotto.SpawnGrotte();
    }

    
    public TileBase GetTile(int i , int j)
    {
        return tileGround.GetTile(new Vector3Int(i - OffsetXY.x, -j - OffsetXY.y));
    }

    public Vector2Int GetPlayerPos(Vector3 Player)
    {
        return (Vector2Int)tileGround.WorldToCell(Player);
    } 
    

    public void InitializeGrid()
    {
        OffsetXY.x = dimensionsX / 2;
        OffsetXY.y = 0;
        int debutStrate = 0;
        LTiles.Clear();
        LtilesVoisin.Clear();
        tileGround.ClearAllTiles();

        for ( int s = 0 ; s < _strates.Count(); s++) 
        {
            if(s > 0) 
            {
                debutStrate += _strates[s - 1].SizeY ;
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
        size = new Vector2Int(dimensionsX, tileGround.size.y);
        float pair = -0.18f;
        if (tileGround.size.y % 2 == 0) pair = 0;
        Vector2 shadowPos = new Vector3(0, -tileGround.CellToWorld(new Vector3Int(0, (size.y/ 2)-1 + OffsetXY.y)).y + pair);
        Vector2 ShadowScale = new Vector3(tileGround.size.x, tileGround.size.y, tileGround.size.z) * 0.32f;

        if (Fog != null)
            Fog.InitShadow(size, shadowPos, ShadowScale);


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
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), st.Tiles[0].Tile);
                }
                else 
                {
                    tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), st.Tiles[1].Tile);
                }
            }
            rng += Random.Range(-1, 2);
            rng = Mathf.Min(rng, start + st.SizeY-1);
            rng = Mathf.Max(rng, start);

            //tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), GetTileFromStrate(_strates[s], x, y));
        }
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

