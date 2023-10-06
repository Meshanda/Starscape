using Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : Slot, IPointerEnterHandler, IPointerExitHandler
{
    #region Variables

    #endregion
    
    #region Events

    public void OnPointerEnter(PointerEventData eventData)
    { 
        if (!CanInteractWithInventory())
            return;

        TooltipSystem.Instance.Show(_itemStack.ItemName, _itemStack.ItemDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!CanInteractWithInventory())
            return;
        
        TooltipSystem.Instance.Hide();
    }

    #endregion

    #region Utils
    private bool CanInteractWithInventory()
    {
        return ItemStack.IsValid(_itemStack) && InventorySystem.Instance.IsInventoryOpen;
    }
    
    #endregion
}
