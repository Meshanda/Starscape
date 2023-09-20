using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : Singleton<World>
{
    [SerializeField] private LayerMask _minableMask;
    
    [SerializeField] private Tilemap _groundTilemap;
    public Tilemap GroundTilemap => _groundTilemap;
    
    [SerializeField] private Tilemap _backgroundTilemap;
    public Tilemap BackGroundTilemap => _backgroundTilemap;
}
