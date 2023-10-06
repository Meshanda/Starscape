using System;
using System.Collections;
using UnityEngine;

public class Placing : MonoBehaviour
{
    [SerializeField] private float _placingDistance = 2.0f;
    [SerializeField] private float _placingDelay = .15f;

    private Vector2 _mousePosition => _cam.ScreenToWorldPoint(Input.mousePosition);
    private float _distanceFromPlayer => Mathf.Abs(Vector2.Distance(transform.position, _mousePosition));

    private Coroutine _placingRoutine;
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Start()
    {
        StartCoroutine(nameof(VisualizeHeldItemRoutine));
    }

    private IEnumerator VisualizeHeldItemRoutine()
    {
        while (true)
        {
            Item selectedItem = InventorySystem.Instance.GetSelectedSlot()?.ItemStack?.GetItem();
            Vector2 worldPos = _cam.ScreenToWorldPoint(Input.mousePosition);

            World.Instance.VisualizeHeldItem(worldPos, selectedItem, _distanceFromPlayer <= _placingDistance);
            
            yield return null;
        }
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
        _placingRoutine = StartCoroutine(PlacingRoutine());
    }

    private void OnPlaceRelease()
    {
        StopCoroutine(_placingRoutine);
    }

    private void PlaceBlock() // input action
    {
        if (_distanceFromPlayer > _placingDistance)
            return;
        
        var slot = InventorySystem.Instance.GetSelectedSlot();
        if (!slot || slot.ItemStack is null)
            return;

        if (World.Instance.TryPlaceTile(slot.ItemStack.itemID, _cam.ScreenToWorldPoint(Input.mousePosition)))
        {
            slot.ItemStackRemoveNumber(1);
        }
    }
}
