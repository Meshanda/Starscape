using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class StrateGeneration : MonoBehaviour
{
    //public int dimensions;
    public Vector2Int dimensionsXY;
    public Vector2Int OffsetXY;

    public bool autoUpdate;

    // public List<Cell> gridComponents;
    // public Cell cellObj;
    [SerializeField] private List<Strate> _strates = new List<Strate>();
    public TileBase tile;
    public Tilemap tileGround;

    public TileBase[] tileObjects;

    public List<TilePos> LTiles = new List<TilePos>();

    int iterations = 0;

    void Awake()
    {
        //gridComponents = new List<Cell>();
        InitializeGrid();
    }
    
    public void InitializeGrid()
    {
        //for (int y = 0; y < dimensionsXY.y; y++)
        //{
        //    for (int x = 0; x < dimensionsXY.x; x++)
        //    {
        //        tileGround.SetTile(new Vector3Int(x, -y), tile);
        //    }
        //}
        int debutStrate = 0;
        for ( int s = 0 ; s < _strates.Count(); s++) 
        {

            if(s > 0) 
            {
                debutStrate += _strates[s - 1].SizeY ;
                Debug.Log(debutStrate);
            }
            for (int y = debutStrate; y < debutStrate+_strates[s].SizeY; y++)
            {
                for (int x = 0; x < dimensionsXY.x; x++)
                {
                    if(_strates[s].SizeY == 2 && y == debutStrate + 1) 
                    {
                        tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), _strates[s].Tiles[1].Tile);
                    }
                    else
                    {
                        tileGround.SetTile(new Vector3Int(x - OffsetXY.x, -y - OffsetXY.y), GetTileFromStrate(_strates[s],x,y));
                    }
                }
            }
        }
        


        //StartCoroutine(CheckEntropy());
    }

   

    public TileBase GetTileFromStrate(Strate strate, int x, int y) 
    {
        float rng = Random.Range(0, 100);
        int pourcentage = 0;
        for (int i = 0; i < strate.Tiles.Count(); i++) 
        {
            pourcentage  += strate.Tiles[i].Pourcentage;
            if (rng < pourcentage) 
            {
                if (strate.Tiles[i].IsMinerai)
                {
                    LTiles.Add(new TilePos(strate.Tiles[i].Tile, x, y));
                }
                return strate.Tiles[i].Tile;
            }
        }
        return strate.Tiles[0].Tile;
    }
}
[Serializable]
public struct Strate 
{
   public TilesStrate[] Tiles;
   public int SizeY;
}

[Serializable]
public struct TilesStrate 
{
    public TileBase Tile;
    public int Pourcentage;
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

