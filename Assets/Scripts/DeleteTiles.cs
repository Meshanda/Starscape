using UnityEngine;
using UnityEngine.Tilemaps;

public class DeleteTiles : MonoBehaviour
{
    [SerializeField] private Tilemap _tilemap;

    [ContextMenu("Clear Tiles")]
    public void ClearTiles()
    {
        _tilemap.ClearAllTiles();
    }
}
