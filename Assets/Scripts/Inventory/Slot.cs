using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class Slot : MonoBehaviour
    {
        [SerializeField] protected Image _itemImage;
        [SerializeField] protected TMP_Text _itemCount;
    
        protected ItemStack _itemStack;
        public virtual ItemStack ItemStack
        {
            get
            {
                if (isActiveAndEnabled)
                {
                    StartCoroutine(RefreshRoutine());
                }
                
                return _itemStack;
            }
            set
            {
                _itemStack = value;
                Refresh();
            }
        }

        public void ItemStackRemoveNumber(int n)
        {
            if (_itemStack is null)
                return;
            
            _itemStack.number -= n;
            if (_itemStack.number <= 0)
                _itemStack = null;
        }
        
        private IEnumerator RefreshRoutine()
        {
            yield return new WaitForEndOfFrame();

            Refresh();
        }

        public void Refresh()
        {
            _itemImage.sprite = _itemStack?.GetItem().sprite;
            _itemCount.text = _itemStack?.number.ToString();

            _itemImage.gameObject.SetActive(_itemStack != null);
            _itemCount.gameObject.SetActive(_itemStack?.number > 1);
        }
    }
}