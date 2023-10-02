using System.Collections;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : Slot, IPointerEnterHandler, IPointerExitHandler
{
 
    #region Variables

    [SerializeField] private GameObject _selection;

    public bool IsSelected => _selection.activeSelf;

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
    public void Select(bool status)
    {
        _selection.SetActive(status);
    }
    
    #endregion
}
