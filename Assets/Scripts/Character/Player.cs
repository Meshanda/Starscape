using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _selectedSprite;
    [SerializeField] private Transform _dropPosition;

    public Transform DropPosition => _dropPosition;

    public void OnQuickSlot1() { InventorySystem.Instance.SelectSlot(0); }
    public void OnQuickSlot2() { InventorySystem.Instance.SelectSlot(1); }
    public void OnQuickSlot3() { InventorySystem.Instance.SelectSlot(2); }
    public void OnQuickSlot4() { InventorySystem.Instance.SelectSlot(3); }
    public void OnQuickSlot5() { InventorySystem.Instance.SelectSlot(4); }
    public void OnQuickSlot6() { InventorySystem.Instance.SelectSlot(5); }
    public void OnQuickSlot7() { InventorySystem.Instance.SelectSlot(6); }
    public void OnQuickSlot8() { InventorySystem.Instance.SelectSlot(7); }
    public void OnQuickSlot9() { InventorySystem.Instance.SelectSlot(8); }
    
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
            if (ItemStack.IsValid(itemStack))
            {
                _selectedSprite.sprite = itemStack?.GetItem().sprite;
            }
            else
            {
                _selectedSprite.sprite = null;
            }
            
            yield return _refreshHeldItemDelay;
        }
    }
}
