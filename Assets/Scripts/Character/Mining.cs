using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class Mining : MonoBehaviour
{
    [SerializeField] private GameObject _dropPrefab;
    [SerializeField] private float _miningDistance = 1.0f;
    private Vector2 _mousePosition => _cam.ScreenToWorldPoint(Input.mousePosition);
    private float _distanceFromPlayer => Mathf.Abs(Vector2.Distance(transform.position, _mousePosition));

    private Camera _cam;
    
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnMine()
    {
        if (_distanceFromPlayer > _miningDistance)
            return;

        var cellPos = World.Instance.GroundTilemap.WorldToCell(_mousePosition);
        var tile = World.Instance.GroundTilemap.GetTile(cellPos);
        if (!tile) return;
        
        World.Instance.GroundTilemap.SetTile(cellPos, null);

        var item = GameManager.Instance.database.GetItemByTile(tile);
        var drop = Instantiate(_dropPrefab,
            World.Instance.GroundTilemap.CellToWorld(cellPos) + new Vector3(0.16f, 0.16f, 0.0f),
            Quaternion.identity).GetComponent<Drop>();
        
        drop.ItemStack = item.GenerateLoot();
        drop.AddRandomForce();
    }
}
