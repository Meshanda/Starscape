using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandSlot : MonoBehaviour
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private TMP_Text _itemCount;
    
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
    
    private void Update()
    {
        transform.position = Input.mousePosition;
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
}
