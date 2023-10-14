using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Mining : MonoBehaviour
{
    [SerializeField] private float _miningDistance = 1.0f;
    [SerializeField] private GameObject _crackPrefab;
    
    private Vector2 _mousePosition => _cam.ScreenToWorldPoint(Input.mousePosition);
    private float _distanceFromPlayer => Mathf.Abs(Vector2.Distance(transform.position, _mousePosition));

    private Camera _cam;
    private Coroutine _miningRoutine;
    private Item _currentMiningItem;
    private (Tilemap, TileBase, Vector3Int) _currentMiningTile;
    
    private float _currentMiningProgress;
    private float CurrentMiningProgress
    {
        get => _currentMiningProgress;
        set
        {
            _currentMiningProgress = value;
            _crackSpriteRenderer?.material?.SetFloat("_Health", _currentMiningProgress);
            _crackObject?.SetActive(_currentMiningProgress > 0.0f);
            if ((value % .5) is 0 && _miningRoutine is not null)
                SoundManager.Instance.PlayBreakingSound();
        }
    }
    
    private GameObject _crackObject;
    private SpriteRenderer _crackSpriteRenderer;
    
    private void Awake()
    {
        _cam = Camera.main;
        _crackObject = Instantiate(_crackPrefab);
        _crackSpriteRenderer = _crackObject.GetComponent<SpriteRenderer>();
        _crackObject.SetActive(false);
    }

    private IEnumerator MiningRoutine()
    {
        while (true)
        {
            MineBlock();
            yield return null;
        }
    }

    private void OnMine()
    {
        if (UsableItem.TryUseItem(InventorySystem.Instance?.GetSelectedSlot()?.ItemStack))
            SoundManager.Instance.PlayTeleportSound(); // TODO pref a clean func inside Player
        _miningRoutine = StartCoroutine(MiningRoutine());
    }
    
    private void OnMineRelease()
    {
        StopAllCoroutines();
        _miningRoutine = null;
        
        CurrentMiningProgress = 0.0f;
    }

    private void MineBlock()
    {
        if (_distanceFromPlayer > _miningDistance)
            return;

        Item selectedItem = InventorySystem.Instance.GetSelectedSlot()?.ItemStack?.GetItem();
        (Tilemap, TileBase, Vector3Int) tile = World.Instance.FindHitTile(_mousePosition, selectedItem);

        if (_currentMiningTile != tile)
        {
            _currentMiningTile = tile;
            CurrentMiningProgress = 0.0f;
            
            if (!_currentMiningTile.Item1)
            {
                return;
            }
            
            _currentMiningItem = GameManager.Instance.database.GetItemByTile(tile.Item2);
            if (_currentMiningItem is null)
            {
                Debug.LogWarning("Unreferenced tile in database!");
                return;
            }
            
            Vector3 newPos = World.Instance.GetWorldCenterOfTile(tile.Item3);
            newPos.z = tile.Item1.transform.position.z;
            _crackObject.transform.position = newPos;
            _crackSpriteRenderer.sprite = _currentMiningItem.sprite;
        }

        if (!_currentMiningTile.Item1)
        {
            return;
        }

        ToolData selectedToolData = selectedItem is not null ? selectedItem.toolData : ToolData.DEFAULT;
        if (selectedToolData.toolStrength.IsEnoughFor(_currentMiningItem.tileInfo.breakingRules.requiredStrength))
        {
            CurrentMiningProgress += (Time.deltaTime * selectedToolData.tileDamagePerSecond) / _currentMiningItem.tileInfo.breakingRules.life;
        }
        else
        {
            CurrentMiningProgress = 0.25f;
            // Play Sound
            return; // do nothing, we can't mine this tile with the item we currently have selected
        }

        if (CurrentMiningProgress >= 1.0f) // destroy tile
        {
            World.Instance.TryDestroyTile(tile.Item1, tile.Item3);
        }
    }
}
