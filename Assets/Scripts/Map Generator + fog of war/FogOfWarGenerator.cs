using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(StrateGeneration))]
public class FogOfWarGenerator : MonoBehaviour
{
    public Material lightShader;
    public SpriteRenderer shadow;
    public List<Torche> LTorche = new List<Torche>();

    [HideInInspector]public StrateGeneration generator;
    private Texture2D wordTilesMap;
    private Texture2D PlayerTexture;
    private Texture2D TorcheTexture;
    private int textureSizeX = 0;
    private int textureSizeY = 0;
    

    private void Awake()
    {
        Mining.OnMineTile += CallEventShadowGround;
        Placing.OnPlaceTile += CallEventShadowGround;
        CharacterController2D.OnMoveEvent += CallEventShadowPlayer;
        generator = GetComponent<StrateGeneration>();
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
    }



    private void CallEventShadowPlayer(Vector3 vector)
    {
        UpdatePlayerLight(vector);
    }
    private void CallEventShadowGround(Item item, Vector2 vector)
    {
        UpdateShadowGround();
        foreach ( Torche T in LTorche)
        {
            if( T.Position == vector)
            {
                CallEventRemoveTorche(T);
            }
        }
    }
    public void InitShadow(Vector2Int Size, Vector2 position, Vector2 scale)
    {
        textureSizeX = Size.x;
        textureSizeY = Size.y;
        ResetShadowGround();
        ResetShadowPlayer();
        ResetShadowTorche();
        shadow.transform.position = position;
        shadow.transform.localScale = scale;
        UpdateShadowGround();
    }

    #region Ground Shadow
    public void ResetShadowGround()
    {
        wordTilesMap = new Texture2D(textureSizeX, textureSizeY);
        wordTilesMap.filterMode = FilterMode.Point;
        lightShader.SetTexture("_ShadowTexture", wordTilesMap);
        for (int i = 0; i < textureSizeX; i++)
        {
            for (int j = 0; j < textureSizeY; j++)
            {
                wordTilesMap.SetPixel(i, j, new Color(1, 0, 0, 1));
            }
        }

        wordTilesMap.Apply();
    }

    public void UpdateShadowGround()
    {
        ResetShadowGround();
        for (int i = textureSizeX; i >= 0; i--)
        {
            for (int j = textureSizeY; j >= 0 ; j--)
            {
                float temp = 1;
                if (generator.GetTile(i, j) != null)
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
        for (int i = 0; i < textureSizeX; i++)
        {
            for (int j = 0; j < textureSizeY; j++)
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
    public void ResetShadowPlayer()
    {
        PlayerTexture = new Texture2D(textureSizeX, textureSizeY);
        PlayerTexture.filterMode = FilterMode.Point;
        lightShader.SetTexture("_PlayerLight", PlayerTexture);
        for (int i = 0; i < textureSizeX; i++)
        {
            for (int j = 0; j < textureSizeY; j++)
            {
                PlayerTexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }

    public void UpdatePlayerLight(Vector3 pos)
    {
        ResetShadowPlayer();
        Vector2Int Cell = generator.GetTilesPos(pos);
        for (int i = -10; i < 11; i++)
        {
            for (int j = -10; j < 11; j++)
            {
                int x = Cell.x + generator.OffsetXY.x + i;
                int y = textureSizeY + Cell.y + j - generator.OffsetXY.y;
                if(y <  textureSizeY && y>=0)
                PlayerTexture.SetPixel(x, y, new Color(1 - (0.1f * Mathf.Abs(i) + 0.1f * Mathf.Abs(j)), 0, 0, 1));
            }
        }
        PlayerTexture.Apply();
    }
    #endregion

    #region Torche Light
    public void ResetShadowTorche()
    {
        TorcheTexture = new Texture2D(textureSizeX, textureSizeY);
        TorcheTexture.filterMode = FilterMode.Point;
        lightShader.SetTexture("_TorcheTexture", TorcheTexture);
        for (int i = 0; i < textureSizeX; i++)
        {
            for (int j = 0; j < textureSizeY; j++)
            {
                TorcheTexture.SetPixel(i, j, new Color(0, 0, 0, 1));
            }
        }
        TorcheTexture.Apply();
    }
    public void UpdateAllTorcheLight()
    {
        ResetShadowTorche();
        foreach (Torche T in LTorche)
        {
            UpdateTorcheLight(T);
        }
    }

    public void UpdateTorcheLight(Torche torche)
    {
        for (int i = -torche.range; i <= torche.range; i++)
        {
            for (int j = -torche.range; j <= torche.range; j++)
            {
                int x = torche.Position.x + generator.OffsetXY.x + i;
                int y = textureSizeY + torche.Position.y + j - generator.OffsetXY.y;
                if (y < textureSizeY && y >= 0)
                    TorcheTexture.SetPixel(x, y, new Color(torche.intensity - ((torche.intensity / torche.range) * Mathf.Abs(i) + (torche.intensity / torche.range) * Mathf.Abs(j)), 0, 0, 1));
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
                int y = textureSizeY + torche.Position.y + j - generator.OffsetXY.y;
                if (y < textureSizeY && y >= 0)
                    TorcheTexture.SetPixel(x, y, new Color(0, 0, 0, 1));
            }
        }
        TorcheTexture.Apply();
    }
    #endregion
}
