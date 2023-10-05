using UnityEngine;
using UnityEngine.Tilemaps;

public class World : Singleton<World>
{
    [SerializeField] private LayerMask _minableMask;
    
    [SerializeField] private Tilemap _groundTilemap;
    public Tilemap GroundTilemap => _groundTilemap;
    
    [SerializeField] private Tilemap _backgroundTilemap;
    public Tilemap BackGroundTilemap => _backgroundTilemap;

    [SerializeField] private PolygonCollider2D _confinerBox;
    public PolygonCollider2D ConfinerBox => _confinerBox;
}
