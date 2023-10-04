using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

[RequireComponent(typeof(StrateGeneration))]
public class FogOfWarGenerator : MonoBehaviour
{
    public Material lightShader;
    public SpriteRenderer shadow;
    private StrateGeneration generator;
    [HideInInspector]public Texture2D wordTilesMap;
    [HideInInspector] public Texture2D PlayerTexture;
    [HideInInspector] public Texture2D TorcheTexture;

    int textureSizeX = 0;
    int textureSizeY = 0;
    

    private void Awake()
    {
        Mining.OnMineTile += CallEventShadowGround;
        Placing.OnPlaceTile += CallEventShadowGround;
        CharacterController2D.OnMoveEvent += CallEventShadowPlayer;
        generator = GetComponent<StrateGeneration>();
    }

    private void CallEventShadowPlayer(Vector3 vector)
    {
        UpdatePlayerLight(vector);
    }
    private void CallEventShadowGround(Item item, Vector2 vector)
    {
        UpdateShadowGround();
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
        Vector2Int Cell = generator.GetPlayerPos(pos);
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

    public void UpdateTorcheLight(Vector3Int cell)
    {
        ResetShadowPlayer();
        TorcheTexture.Apply();
    }
    #endregion
}
