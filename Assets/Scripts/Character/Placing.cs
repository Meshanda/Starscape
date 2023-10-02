using System;
using UnityEngine;

public class Placing : MonoBehaviour
{
    [SerializeField] private float _placingDistance = 2.0f;
    
    private Vector2 _mousePosition => _cam.ScreenToWorldPoint(Input.mousePosition);
    private float _distanceFromPlayer => Mathf.Abs(Vector2.Distance(transform.position, _mousePosition));
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnPlace()
    {
        if (_distanceFromPlayer > _placingDistance)
            return;
        
        var cellPos = World.Instance.GroundTilemap.WorldToCell(_cam.ScreenToWorldPoint(Input.mousePosition));
        var slot = InventorySystem.Instance.GetSelectedSlot();
        if (!slot || slot.ItemStack is null)
            return;

        if (TileCanBePlaced(cellPos)) 
        {
            World.Instance.GroundTilemap.SetTile(cellPos, slot.ItemStack.GetItem().tile);
            slot.ItemStackRemoveNumber(1);
        }
    }
    
    private bool TileCanBePlaced(Vector3Int cellPos)
    {
        var hit = Physics2D.Raycast(_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity);
        if (hit.collider != null && hit.collider.gameObject.tag.Equals("Player"))
        {
            Debug.Log("Hit player!");
            return false;
        }
        
        if (World.Instance.GroundTilemap.GetTile(cellPos) is not null)
        {
            Debug.Log("Tile is occupied.");
            return false;
        }

        if (World.Instance.BackGroundTilemap.GetTile(cellPos) is not null)
        {
            Debug.Log("Background found!");
            return true;
        }

        var cellAbove = cellPos + new Vector3Int(0, 1);
        var cellBelow = cellPos + new Vector3Int(0, -1);
        var cellLeft = cellPos + new Vector3Int(-1, 0);
        var cellRight = cellPos + new Vector3Int(1, 0);

        if (World.Instance.GroundTilemap.GetTile(cellBelow) is not null ||
            World.Instance.GroundTilemap.GetTile(cellLeft) is not null ||
            World.Instance.GroundTilemap.GetTile(cellRight) is not null ||
            World.Instance.GroundTilemap.GetTile(cellAbove) is not null)
        {
            // Debug.Log($" Below:{World.Instance.GroundTilemap.GetTile(cellBelow) is not null}" +
            //           $"Left: {World.Instance.GroundTilemap.GetTile(cellLeft) is not null}" +
            //           $"Right: {World.Instance.GroundTilemap.GetTile(cellRight) is not null}" +
            //           $"Above: {World.Instance.GroundTilemap.GetTile(cellAbove) is not null}");
            return true;
        }

        return false;
    }
}
