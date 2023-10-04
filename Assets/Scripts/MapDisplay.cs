using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int hauteur = noiseMap.GetLength(0);
        int largeur = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(hauteur, largeur);

        Color[] colourMap = new Color[hauteur * largeur];

        for( int y = 0; y < largeur; y++)
        {
            for (int x = 0; x < hauteur; x++)
            {
                colourMap[y * hauteur + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        texture.SetPixels(colourMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(largeur,1, hauteur);
    }
}
