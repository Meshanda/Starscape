using System.Collections;
using Inventory;
using UnityEngine;
using UnityEngine.EventSystems;

public class CraftingSlot : Slot, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    private CraftRecipe _recipe;
    private Coroutine _craftRoutine;
    private InventorySystem _inv;

    public CraftRecipe Recipe
    {
        get => _recipe;
        set
        {
            _recipe = value;
            ItemStack = _recipe.itemCrafted;
        }
    }

    private void Start()
    {
        _inv = InventorySystem.Instance;
    }
    
    public void OnClick()
    {
        InventorySystem.Instance.OnClickCraftSlot(this);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _craftRoutine = StartCoroutine(CraftRoutine());
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_craftRoutine is not null)  
            StopCoroutine(_craftRoutine);
    }

    public IEnumerator CraftRoutine()
    {
        var delay = _inv._splitInitialDelay;
        
        while (true)
        {
            OnClick();
            yield return new WaitForSeconds(delay);
            delay = Mathf.Max(delay - _inv._splitStep, _inv._splitMinDelay);
        }
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
