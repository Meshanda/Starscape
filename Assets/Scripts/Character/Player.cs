using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _selectedSprite;
    [SerializeField] private Transform _dropPosition;

    public Transform DropPosition => _dropPosition;

    public void OnToggleInventory()
    {
        InventorySystem.Instance.ToggleInventory();
    }
    
    public void OnInventoryScroll(InputValue value)
    {
        var scrollValue = value.Get<float>();
        switch (scrollValue)
        {
            case > 0:
                if (InventorySystem.Instance.IsInventoryOpen)
                    InventorySystem.Instance.SelectPreviousCraftSlot();
                else    
                    InventorySystem.Instance.SelectPreviousSlot();
                break;
            
            case < 0:
                if (InventorySystem.Instance.IsInventoryOpen)
                    InventorySystem.Instance.SelectNextCraftSlot();
                else    
                    InventorySystem.Instance.SelectNextSlot();
                break;
        }
    }

    private WaitForSeconds _refreshHeldItemDelay = new WaitForSeconds(0.1f);
    
    private void Start()
    {
        StartCoroutine(nameof(RefreshHeldItemRoutine));
    }

    public IEnumerator RefreshHeldItemRoutine()
    {
        while (true)
        {
            var itemStack = InventorySystem.Instance.GetSelectedSlot()?.ItemStack;
            _selectedSprite.sprite = itemStack?.GetItem().sprite;
            yield return _refreshHeldItemDelay;
        }
    }
}
