using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class Minimap : MonoBehaviour
    {
        [SerializeField] private int defaultZoomValue;
        [SerializeField] private int minZoomValue;
        [SerializeField] private int maxZoomValue;

        private int _currentZoomValue;

        public static event Action<int> ChangeMinimapZoom;
        private void Start()
        {
            _currentZoomValue = defaultZoomValue;
            ChangeMinimapZoom?.Invoke(defaultZoomValue);
        }

        public void ClickMinus()
        {
            _currentZoomValue++;
            if (_currentZoomValue > maxZoomValue)
                _currentZoomValue = maxZoomValue;

            ChangeMinimapZoom?.Invoke(_currentZoomValue);
            SoundManager.Instance.PlayClickSound();
            GameManager.Instance.LoseFocus();
        }
        
        public void ClickPlus()
        {
            _currentZoomValue--;
            if (_currentZoomValue < minZoomValue) 
                _currentZoomValue = minZoomValue;
            
            ChangeMinimapZoom?.Invoke(_currentZoomValue);
            SoundManager.Instance.PlayClickSound();
            GameManager.Instance.LoseFocus();
        }
    }
}