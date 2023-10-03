using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
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
}
