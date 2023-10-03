using Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingSlot : Slot, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _selected;
    private CraftRecipe _recipe;

    public CraftRecipe Recipe
    {
        get => _recipe;
        set
        {
            _recipe = value;
            ItemStack = _recipe.itemCrafted;
        }
    }
    
    public void OnClick()
    {
        InventorySystem.Instance.OnClickCraftSlot(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Instance.Show(_recipe.itemCrafted.ItemName, _recipe.itemCrafted.ItemDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance.Hide();
    }
}
