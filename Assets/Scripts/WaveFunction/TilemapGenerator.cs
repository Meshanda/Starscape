using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGenerator : MonoBehaviour
{
    [SerializeField]
    private Tilemap GroundTilemap;

    [SerializeField] 
    private TileBase GroundTile;

    public void PaintGroundTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, GroundTilemap, GroundTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)position);
            tilemap.SetTile(tilePosition, tile);
        }
    }
}
