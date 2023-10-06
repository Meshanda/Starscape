using UnityEngine;


public class Torche : MonoBehaviour
{
    public Vector2Int Position;
    public float intensity = 0.8f;
    public int range = 5;
    FogOfWarGenerator fog;

    void Start()
    {
        fog = FindAnyObjectByType<FogOfWarGenerator>();
        Position = fog.generator.GetTilesPos(transform.position);
        Position.y--;
        fog.CallEventTorche(this);
    }

    private void OnDestroy()
    {
        fog.CallEventRemoveTorche(this);
    }
}
