using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapHauteur;
    public int mapLargeur;
    public float noiseScale;

    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noisMap = Noise.GenerateNoise(mapHauteur, mapLargeur, noiseScale);
        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        display.DrawNoiseMap(noisMap);
    }
}
