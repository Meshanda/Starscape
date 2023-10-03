using UnityEngine.EventSystems;

namespace Inventory
{
    public class ReadOnlySlot : Slot, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!InventorySystem.Instance.IsInventoryOpen)
                return;
        
            TooltipSystem.Instance.Show(_itemStack.ItemName, _itemStack.ItemDescription);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!InventorySystem.Instance.IsInventoryOpen)
                return;
        
            TooltipSystem.Instance.Hide();
        }
    }
}