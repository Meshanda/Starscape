using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoise(int mapHaut, int MapLargeur , float scale)
    {
        float[,] noiseMap = new float[MapLargeur, mapHaut];

        if(scale <= 0)
        {
            scale = 0.00001f;
        }

        for(int y = 0; y < mapHaut; y++)
        {
            for (int x = 0; x < MapLargeur; x++)
            {
                float sampleX = x / scale;
                float sampleY = y / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }
        }
        return noiseMap;
    }
}
