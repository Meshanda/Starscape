using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
using Random = UnityEngine.Random;

public class StrateGeneration : MonoBehaviour
{

    [Header("Initialisation")]
    public int dimensionsX;
    private Vector2Int OffsetXY;
    public Tilemap tileGround;
    [SerializeField] private List<Strate> _strates = new List<Strate>();
    public TileBase tileBase;

    [Header("Filon")]
    public List<TilePos> LTiles = new List<TilePos>();
    public List<TilePos> LtilesVoisin = new List<TilePos>();
    public float ProbVoisin;
    public float ProbVoisinVoisin;

    [Header("Auto Update Generator")]
    public bool autoUpdate;
    [SerializeField] private PasteGrotte _pasteGrotto;

    void Awake()
    {
        InitializeGrid();
        if(_pasteGrotto != null)
            _pasteGrotto.SpawnGrotte();
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

        // for(int i = 0; i < dimensionsX ; i++)
        // {
        //     tileGround.SetTile(new Vector3Int(i - OffsetXY.x, 0 - OffsetXY.y), tileBase);
        // }


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

