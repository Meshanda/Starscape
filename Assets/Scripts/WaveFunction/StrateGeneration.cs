using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;
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

    void Awake()
    {
        
        InitializeGrid();
        if(_pasteGrotto != null)
            _pasteGrotto.SpawnGrotte(); 

    }

    public void InitializeGrid()
    {
        OffsetXY.x = dimensionsX / 2;
        OffsetXY.y = 6;
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
        ResetShadow();
        shadow.transform.position = new Vector3(0, -tileGround.CellToWorld(new Vector3Int(0, (sizeY / 2) + OffsetXY.y) ).y + 0.17f);
        shadow.transform.localScale = new Vector3(tileGround.size.x, -tileGround.size.y, tileGround.size.z) * 0.32f;
        UpdateShadow();
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


    public void ResetShadow()
    {
        wordTilesMap = new Texture2D(dimensionsX, sizeY);
        wordTilesMap.filterMode = FilterMode.Point;
        PlayerTexture = new Texture2D(dimensionsX, sizeY);
        PlayerTexture.filterMode = FilterMode.Point;
        lightShader.SetTexture("_ShadowTexture", wordTilesMap);
        lightShader.SetTexture("_PlayerLight", PlayerTexture);
        for (int i = 0; i < dimensionsX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                wordTilesMap.SetPixel(i, j, new Color(1, 0, 0, 1));
                PlayerTexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }
        wordTilesMap.Apply();
        PlayerTexture.Apply();
    }

    public void UpdatePlayerLight()
    {
        Vector3Int Cell = tileGround.WorldToCell(Player.transform.position);
        for (int i = -5; i < 6; i++)
        {
            for (int j = -5; j < 6; j++)
            {
                Debug.Log("ici" +Cell);
                PlayerTexture.SetPixel(Cell.x + i,Cell.y + j, new Color(1, 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }


    public void UpdateShadow()
    {
        for (int i = 0; i < dimensionsX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (tileGround.GetTile(new Vector3Int(i - OffsetXY.x, -j - OffsetXY.y)) != null)
                {
                    wordTilesMap.SetPixel(i, j, new Color(wordTilesMap.GetPixel(i, j - 1).r - 0.2f, 0, 0, 1));

                }
                else
                {
                    wordTilesMap.SetPixel(i, j, new Color(1, 0, 0, 1));
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

