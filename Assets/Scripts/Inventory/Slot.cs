using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Inventory
{
    public class Slot : MonoBehaviour
    {
        [SerializeField] private Image _slotImg;
        
        [SerializeField] protected Image _itemImg;
        [SerializeField] protected TMP_Text _itemCount;
        
        [Header("Normal Settings")]
        [SerializeField] protected Color _normalColor = Color.white;
        [SerializeField] protected Vector3 _normalScale = Vector3.one;
        [SerializeField] protected float _normalTime = .5f;

        [Header("Selected Settings")] 
        [SerializeField] protected Color _selectedColor = Color.magenta;
        [SerializeField] protected Vector3 _selectedScale = new (1.1f,1.1f,1.1f);
        [SerializeField] protected float _selectTime = .5f;
        
        private Tween _currentTween;
        public event Action<ItemStack> slotChanged;

        public bool IsSelected { get; protected set; }

        protected ItemStack _itemStack;
        public virtual ItemStack ItemStack
        {
            get
            {
                if (isActiveAndEnabled && ItemStack.IsValid(_itemStack)) 
                {
                    StartCoroutine(RefreshRoutine());
                }
                
                return _itemStack;
            }
            set
            {
                _itemStack = value;
                Refresh();
                slotChanged?.Invoke(value);
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
            _itemImg.sprite = _itemStack?.GetItem().sprite;
            _itemCount.text = _itemStack?.number.ToString();

            _itemImg.gameObject.SetActive(_itemStack != null);
            _itemCount.gameObject.SetActive(_itemStack?.number > 1);
        }

        public virtual void Select(bool status)
        {
            switch (status) 
            {
                case true:
                    _currentTween.Kill();
                    _slotImg.color = _selectedColor;
                    _currentTween = _slotImg.gameObject.transform.parent.DOScale(_selectedScale, _selectTime).SetEase(Ease.InOutSine);
                    IsSelected = true;
                    break;
                case false:
                    _currentTween.Kill();
                    _slotImg.color = _normalColor;
                    _currentTween = _slotImg.gameObject.transform.parent.DOScale(_normalScale, _normalTime).SetEase(Ease.InOutSine);
                    IsSelected = false;
                    break;
            }
        }
    }
}