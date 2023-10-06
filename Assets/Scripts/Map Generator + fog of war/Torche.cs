using UnityEngine;


public class Torche : MonoBehaviour
{
    public Vector2Int Position;
    public float intensity;
    public int range;

    void Start()
    {
        FogOfWarGenerator fog = FindAnyObjectByType<FogOfWarGenerator>();
        Position = fog.generator.GetTilesPos(transform.position);
        Position.y--;
        intensity = 0.8f;
        range = 5;
        fog.CallEventTorche(this);
    }
}
