using System;
using System.Collections;
using UI;
using UnityEngine;

public class Placing : MonoBehaviour
{
    [SerializeField] private float _placingDistance = 2.0f;
    [SerializeField] private float _placingDelay = .15f;

    private Vector2 _mousePosition => _cam.ScreenToWorldPoint(Input.mousePosition);
    private float _distanceFromPlayer => Mathf.Abs(Vector2.Distance(transform.position, _mousePosition));

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        // refresh visualized item continuously
        Item selectedItem = InventorySystem.Instance.GetSelectedSlot()?.ItemStack?.GetItem();
        Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);

        World.Instance.VisualizeHeldItem(worldPos, selectedItem, _distanceFromPlayer <= _placingDistance);
    }

    private IEnumerator PlacingRoutine()
    {
        while (true)
        {
            PlaceBlock();
            yield return new WaitForSeconds(_placingDelay);
        }
    }

    private void OnPlace()
    {
        StartCoroutine(PlacingRoutine());
    }

    private void OnPlaceRelease()
    {
        StopAllCoroutines();
    }

    private void PlaceBlock() // input action
    {
        if (_distanceFromPlayer > _placingDistance)
            return;
        
        if (CheckChest()) return;

        var slot = InventorySystem.Instance.GetSelectedSlot();
        if (!slot || slot.ItemStack is null)
            return;
        
        if (World.Instance.TryPlaceTile(slot.ItemStack.itemID, _cam.ScreenToWorldPoint(Input.mousePosition)))
        {
            slot.ItemStackRemoveNumber(1);
        }
    }

    private bool CheckChest()
    {
        var hits = Physics2D.RaycastAll(_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity);

        foreach (var hit in hits)
        {
            if (hit.collider is not null && hit.collider.TryGetComponent(out Chest chest))
            {
                chest.ClickChest();
                return true;
            }
        }

        return false;
    }
}
