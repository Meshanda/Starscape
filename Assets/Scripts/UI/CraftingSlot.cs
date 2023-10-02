using System;
using Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingSlot : Slot, IPointerDownHandler
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

    public void Select(bool status)
    {
        _selected.SetActive(status);
    }
    
    public void OnClick()
    {
        InventorySystem.Instance.SelectCraftSlot(this);
        InventorySystem.Instance.TryCraft(_recipe);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick();
    }
}
