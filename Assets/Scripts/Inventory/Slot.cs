using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class Slot : MonoBehaviour
    {
        [SerializeField] private Image _slotBackground;
        [SerializeField] protected Image _itemImage;
        [SerializeField] protected TMP_Text _itemCount;
        
        [Header("Normal Settings")]
        [SerializeField] protected Color _normalColor = Color.white;
        [SerializeField] protected Vector3 _normalScale = Vector3.one;

        [Header("Selected Settings")] 
        [SerializeField] protected Color _selectedColor = Color.magenta;
        [SerializeField] protected Vector3 _selectedScale = new Vector3(1.1f,1.1f,1.1f);

        public bool IsSelected { get; protected set; }

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

        public virtual void Select(bool status)
        {
            switch (status)
            {
                case true:
                    _slotBackground.color = _selectedColor;
                    IsSelected = true;
                    
                    break;
                case false:
                    _slotBackground.color = _normalColor;
                    IsSelected = false;
                    
                    break;
            }
        }
    }
}