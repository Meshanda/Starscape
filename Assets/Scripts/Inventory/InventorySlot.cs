using Inventory;
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
        return _itemStack is not null && InventorySystem.Instance.IsInventoryOpen;
    }
    
    #endregion
}
