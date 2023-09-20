using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class MiningSystem : MonoBehaviour
{
    [SerializeField] private GameObject _dropPrefab;
    [SerializeField] private LayerMask _minableMask;
    [FormerlySerializedAs("_tilemap")] [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _backgroundTilemap;
    [SerializeField] private TileBase _tile;

    [SerializeField] private float _miningDistance = 2.0f;

    private Vector2 _mousePosition => _cam.ScreenToWorldPoint(Input.mousePosition);
    private float _distanceFromPlayer => Mathf.Abs(Vector2.Distance(transform.position, _mousePosition));

    
    private Camera _cam;
    
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        Mining();
        Placing();
        PickUpTile();
    }

    private void PickUpTile()
    {
        if (_distanceFromPlayer > _miningDistance)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            var cellPos = _groundTilemap.WorldToCell(_cam.ScreenToWorldPoint(Input.mousePosition));
            var tile = _groundTilemap.GetTile(cellPos);
            _tile = tile;
        }
    }
 
    private void Placing()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            var cellPos = _groundTilemap.WorldToCell(_cam.ScreenToWorldPoint(Input.mousePosition));
            if (TileCanBePlaced(cellPos))
            {
                _groundTilemap.SetTile(cellPos, _tile);
                Debug.Log($"{_tile.name} placed.");
            }
            else 
                Debug.LogWarning("NOPE!");
        }
    }

    private bool TileCanBePlaced(Vector3Int cellPos)
    {
        if (_distanceFromPlayer > _miningDistance)
            return false;

        if (_groundTilemap.GetTile(cellPos) is not null)
        {
            Debug.Log("Tile is occupied.");
            return false;
        }

        if (_backgroundTilemap.GetTile(cellPos) is not null)
        {
            Debug.Log("Background found!");
            return true;
        }

        var cellAbove = cellPos + new Vector3Int(0, 1);
        var cellBelow = cellPos + new Vector3Int(0, -1);
        var cellLeft = cellPos + new Vector3Int(-1, 0);
        var cellRight = cellPos + new Vector3Int(1, 0);

        if (_groundTilemap.GetTile(cellBelow) is not null ||
            _groundTilemap.GetTile(cellLeft) is not null ||
            _groundTilemap.GetTile(cellRight) is not null ||
            _groundTilemap.GetTile(cellAbove) is not null)
        {
            Debug.Log($" Below:{_groundTilemap.GetTile(cellBelow) is not null}" +
                      $"Left: {_groundTilemap.GetTile(cellLeft) is not null}" +
                      $"Right: {_groundTilemap.GetTile(cellRight) is not null}" +
                      $"Above: {_groundTilemap.GetTile(cellAbove) is not null}");
            return true;
        }

        return false;
    }

    private void Mining()
    {
        if (_distanceFromPlayer > _miningDistance)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var cellPos = _groundTilemap.WorldToCell(_mousePosition);
            var tile = _groundTilemap.GetTile(cellPos);

            if (tile)
            {
                _groundTilemap.SetTile(cellPos, null);

                var dropGO = Instantiate(_dropPrefab, _groundTilemap.CellToWorld(cellPos) + new Vector3(0.16f, 0.16f, 0.0f), Quaternion.identity);
                var drop = dropGO.GetComponent<Drop>();
                var item = GameManager.Instance.database.GetItemByTile(tile);
                drop.ItemStack = item.GenerateLoot();
            }
        }
    }
}
