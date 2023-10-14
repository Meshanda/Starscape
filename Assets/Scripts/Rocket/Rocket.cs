using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Rocket : MonoBehaviour
{
    [SerializeField] private Tilemap _platformMap;
    [SerializeField] private Transform _platformPos;


    private Tilemap GroundTile => World.Instance.GroundTilemap;

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        Debug.Log(LayerMask.GetMask("Minable"));
        if (other.gameObject.layer != LayerMask.NameToLayer("Minable"))
            return; 
        
        CreatePlatform();

        Destroy(this);
    }

    public void CreatePlatform()
    {
        var pos = GroundTile.WorldToCell(_platformPos.position);   
        
        for (int i = _platformMap.cellBounds.min.x - 1; i < _platformMap.cellBounds.max.x + 1; i++)
        {
            for (int j = _platformMap.cellBounds.min.y - 1; j < _platformMap.cellBounds.max.y + 1; j++)
            {
                TileBase tile = _platformMap.GetTile(new Vector3Int(i, j));
                if (tile != null) 
                {
                    GroundTile.SetTile(new Vector3Int(i, j) + pos, tile);
                }
            }
        } 
    }
}
