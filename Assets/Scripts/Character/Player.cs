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
                InventorySystem.Instance.SelectPreviousSlot();
                break;
            case < 0:
                InventorySystem.Instance.SelectNextSlot();
                break;
        }
    }
}
