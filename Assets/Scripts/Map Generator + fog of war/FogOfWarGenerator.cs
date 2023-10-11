using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(StrateGeneration))]
public class FogOfWarGenerator : MonoBehaviour
{
    public Material lightShader;
    public SpriteRenderer shadow;
    public List<Torche> LTorche = new List<Torche>();
    private Vector2Int playerCell = new Vector2Int(1000, 1000);
    [HideInInspector]public StrateGeneration generator;
    private Texture2D wordTilesMap;
    private Texture2D PlayerTexture;
    private Texture2D TorcheTexture;
    private Vector2Int textureSize;



    private void Awake()
    {
        generator = GetComponent<StrateGeneration>();
    }

    private void OnEnable()
    {
        CharacterController2D.OnMoveEvent += CallEventShadowPlayer;
        World.OnMineTile += CallEventShadowGround;
        World.OnPlaceTile += CallEventShadowGround;
    }

    private void OnDisable()
    {
        CharacterController2D.OnMoveEvent -= CallEventShadowPlayer;
        World.OnMineTile -= CallEventShadowGround;
        World.OnPlaceTile -= CallEventShadowGround;
    }

    public void CallEventTorche(Torche torche)
    {
        LTorche.Add(torche);
        UpdateTorcheLight(torche);
    }
    public void CallEventRemoveTorche(Torche torche)
    {
        RemoveTocheShadow(torche);
        LTorche.Remove(torche);
        UpdateAllTorcheLight();
    }
    private void CallEventShadowPlayer(Vector3 vector)
    {
        UpdatePlayerLight(vector);
    }
    private void CallEventShadowGround(Tilemap tilemaps, Vector3Int vector, Item item)
    {
        UpdateTileGround((Vector2Int) vector);
    }
    public void InitShadow(Vector2Int SizeXY, Vector2 position, Vector2 scale)
    {
        textureSize = SizeXY;
        shadow.transform.position = position;
        shadow.transform.localScale = scale;

        InitShadowGround();
        InitShadowPlayer();
        InitShadowTorche();


        UpdateAllShadowGround();
    }

    #region Ground Shadow
    public void InitShadowGround()
    {
        wordTilesMap = new Texture2D(textureSize.x, textureSize.y);
        wordTilesMap.filterMode = FilterMode.Point;
        lightShader.SetTexture("_ShadowTexture", wordTilesMap);
        for (int i = 0; i < textureSize.x; i++)
        {
            for (int j = 0; j < textureSize.y; j++)
            {
                wordTilesMap.SetPixel(i, j, new Color(1, 0, 0, 1));
            }
        }

        wordTilesMap.Apply();
    }

    public void UpdateTileGround(Vector2Int pos)
    {
        
        for (int i = 10; i >= - 11; i--)
        {
            for (int j = 10; j >= - 11; j--)
            {
                int x = pos.x + generator.OffsetXY.x + i;
                int y = textureSize.y + pos.y + j - generator.OffsetXY.y;
                float temp = 1;
                if (generator.GetTile(x,y) != null && y != textureSize.y - 1)
                {
                    temp = wordTilesMap.GetPixel( x, y + 1).r - 0.2f;
                }
                else
                {
                    temp = 1;
                }
                if (y < textureSize.y && y >= 0)
                {
                    wordTilesMap.SetPixel(x, y, new Color(temp, 0, 0, 1));
                }
            }
        }
        for (int i = 10; i >= -11; i--)
        {
            for (int j = 10; j >= -11; j--)
            {
                int x = pos.x + generator.OffsetXY.x + i;
                int y = textureSize.y + pos.y + j - generator.OffsetXY.y;
                if (generator.GetTile( x +1, y) == null || generator.GetTile(x -1, y) == null)
                {
                    wordTilesMap.SetPixel(x, y, new Color(wordTilesMap.GetPixel(x, y).r + 0.3f, 0, 0, 1));
                }
            }
        }
        wordTilesMap.Apply();
    }
    public void UpdateAllShadowGround()
    {
        for (int i = textureSize.x-1; i >= 0  ; i--)
        {
            for (int j = textureSize.y-1; j >= 0  ; j--)
            {
                float temp;
                if (generator.GetTile(i, j) != null &&  j != textureSize.y-1)
                {
                    temp = wordTilesMap.GetPixel(i, j + 1).r - 0.2f;

                }
                else
                {
                    temp = 1;
                }
                wordTilesMap.SetPixel(i, j, new Color(temp, 0, 0, 1));
            }
        }
        for (int i = 0; i < textureSize.x; i++)
        {
            for (int j = 0; j < textureSize.y; j++)
            {
                if (generator.GetTile(i + 1, j) == null || generator.GetTile(i - 1, j) == null)
                {
                    wordTilesMap.SetPixel(i, j, new Color(wordTilesMap.GetPixel(i, j).r + 0.3f, 0, 0, 1));
                }
            }
        }
        wordTilesMap.Apply();
    }
    #endregion

    #region Player Light
    public void InitShadowPlayer()
    {
        PlayerTexture = new Texture2D(textureSize.x, textureSize.y);
        PlayerTexture.filterMode = FilterMode.Point;
        lightShader.SetTexture("_PlayerLight", PlayerTexture);
        for (int i = 0; i < textureSize.x; i++)
        {
            for (int j = 0; j < textureSize.y; j++)
            {
                PlayerTexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }
    public void ResetOldPosPlayer(Vector2Int pos)
    {
        for (int i = -10; i < 11; i++)
        {
            for (int j = -10; j < 11; j++)
            {
                PlayerTexture.SetPixel(pos.x+i, pos.y+j, new Color(0, 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }

    public void UpdatePlayerLight(Vector3 pos)
    {
        Vector2Int Cell = generator.GetTilesPos(pos);
        if (Cell == playerCell)
        {
            return;
        }
        else
        {
            ResetOldPosPlayer(playerCell);
            playerCell = Cell;
        }
        for (int i = -10; i < 11; i++)
        {
            for (int j = -10; j < 11; j++)
            {
                int x = Cell.x + generator.OffsetXY.x + i;
                int y = textureSize.y + Cell.y + j - generator.OffsetXY.y;
                if(y <  textureSize.y && y>=0)
                PlayerTexture.SetPixel(x, y, new Color(1 - (0.1f * Mathf.Abs(i) + 0.1f * Mathf.Abs(j)), 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }
    #endregion

    #region Torche Light
    public void InitShadowTorche()
    {
        TorcheTexture = new Texture2D(textureSize.x, textureSize.y);
        TorcheTexture.filterMode = FilterMode.Point;
        lightShader.SetTexture("_TorcheTexture", TorcheTexture);
        for (int i = 0; i < textureSize.x; i++)
        {
            for (int j = 0; j < textureSize.y; j++)
            {
                TorcheTexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }
        TorcheTexture.Apply();
    }
    public void UpdateAllTorcheLight()
    {
        //InitShadowTorche();
        foreach (Torche T in LTorche)
        {
            UpdateTorcheLight(T);
        }
    }

    public void UpdateTorcheLight(Torche torche)
    {
        float C = 0;
        for (int i = -torche.range; i <= torche.range; i++)
        {
            for (int j = -torche.range; j <= torche.range; j++)
            {
                int x = torche.Position.x + generator.OffsetXY.x + i;
                int y = textureSize.y + torche.Position.y + j - generator.OffsetXY.y;
                if (y < textureSize.y && y >= 0 && x < textureSize.x && x >= 0 && j <= torche.range - i && i <= torche.range - j)
                    C = TorcheTexture.GetPixel(x,y).r + torche.intensity - ((torche.intensity / torche.range) * Mathf.Abs(i) + (torche.intensity / torche.range) * Mathf.Abs(j));
                    TorcheTexture.SetPixel(x, y, new Color(C, 0, 0, 1));
            }
        }
        TorcheTexture.Apply();
    }
    public void RemoveTocheShadow(Torche torche)
    {
        for (int i = -torche.range; i <= torche.range; i++)
        {
            for (int j = -torche.range; j <= torche.range; j++)
            {
                int x = torche.Position.x + generator.OffsetXY.x + i;
                int y = textureSize.y + torche.Position.y + j - generator.OffsetXY.y;
                if (y < textureSize.y && y >= 0 && x < textureSize.x && x >= 0 && j <= torche.range - i && i <= torche.range - j)
                    TorcheTexture.SetPixel(x, y, new Color(0, 0, 0, 1));
            }
        }
        TorcheTexture.Apply();
    }
    #endregion
}
