using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    //, IDragHandler, IEndDragHandler, IBeginDragHandler
{
 
    #region Variables

    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemCount;
    [SerializeField] private GameObject _selection;
    [SerializeField] private Transform _item;
    public bool ReadOnly;
    
    [HideInInspector] public Vector2Int pos;

    private Transform _previousParent;
    private Vector3 _previousPos;
    private InventorySlot _previousTarget;

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
            Refresh();
        }
    }

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
    
    // public void OnDrag(PointerEventData eventData)
    // {
    //     if (!CanInteractWithInventory() || ReadOnly)
    //         return;
    //     
    //     Debug.Log("Drag");
    //     
    //     _item.position = eventData.position;
    //     var hit = Physics2D.Raycast(eventData.position, Vector2.zero);
    //     
    //     _previousTarget?.Select(false);
    //     
    //     if (hit.collider && hit.collider.transform.TryGetComponent(out InventorySlot slot))
    //     {
    //         if (slot == this) return;
    //         
    //         _previousTarget = slot;
    //         slot.Select(true);
    //     }
    // }
    //
    // public void OnBeginDrag(PointerEventData eventData)
    // {
    //     if (!CanInteractWithInventory() || ReadOnly)
    //         return;
    //     
    //     Debug.Log("BeginDrag");
    //     
    //     _previousParent = transform;
    //     _previousPos = transform.position;
    //
    //     _item.SetParent(InventorySystem.Instance.dragAndDropCanvas);
    // }
    //
    // public void OnEndDrag(PointerEventData eventData)
    // {
    //     if (!CanInteractWithInventory() || ReadOnly)
    //         return;
    //     
    //     Debug.Log("EndDrag");
    //
    //     _item.SetParent(_previousParent);
    //     _item.position = _previousPos;
    //
    //     if (CanChangeSlot())
    //     {
    //         if (_previousTarget.ItemStack is not null && _previousTarget.ItemStack.itemID.Equals(ItemStack.itemID))
    //             _previousTarget.ItemStack.Add(ItemStack.number);
    //         else
    //             _previousTarget.ItemStack = ItemStack;
    //         
    //         ItemStack = null;
    //     }
    //
    //     _previousTarget.Select(false);
    // }

    

    #endregion

    #region Utils

    private bool CanChangeSlot()
    {
        return _previousTarget.IsSelected && (_previousTarget.ItemStack is null || _previousTarget.ItemStack.itemID.Equals(ItemStack.itemID));
    }
    
    private bool CanInteractWithInventory()
    {
        return _itemStack is not null && InventorySystem.Instance.IsInventoryOpen;
    }
    public void Select(bool status)
    {
        _selection.SetActive(status);
    }

    private IEnumerator RefreshRoutine()
    {
        yield return new WaitForEndOfFrame();

        Refresh();
    }

    private void Refresh()
    {
        _itemImage.sprite = _itemStack?.GetItem().sprite;
        _itemCount.text = _itemStack?.number.ToString();

        _itemImage.gameObject.SetActive(_itemStack != null);
        _itemCount.gameObject.SetActive(_itemStack?.number > 1);
    }

    #endregion
}
