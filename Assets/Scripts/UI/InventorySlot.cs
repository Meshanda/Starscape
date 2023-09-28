using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemCount;
    [SerializeField] private GameObject _selection;
    
    [HideInInspector] public Vector2Int pos;

    private ItemStack _itemStack;
    public ItemStack ItemStack
    {
        get
        {
            StartCoroutine(RefreshRoutine());
            return _itemStack;
        }
        set
        {
            _itemStack = value;
            StartCoroutine(RefreshRoutine());
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_itemStack is null || !InventorySystem.Instance.IsInventoryOpen)
            return;
        
        TooltipSystem.Instance.Show(_itemStack.ItemName, _itemStack.ItemDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_itemStack is null || !InventorySystem.Instance.IsInventoryOpen)
            return;
        
        TooltipSystem.Instance.Hide();
    }
    
    public void Select(bool status)
    {
        _selection.SetActive(status);
    }

    private IEnumerator RefreshRoutine()
    {
        yield return new WaitForEndOfFrame();
        
        _itemImage.sprite = _itemStack?.GetItem().sprite;
        _itemCount.text = _itemStack?.number.ToString();
        
        _itemImage.gameObject.SetActive(_itemStack != null);
        _itemCount.gameObject.SetActive(_itemStack?.number > 1);
    }

    
}
