using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class Selector : MonoBehaviour
    {
        [SerializeField] private List<string> _options;
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private UnityEvent<int> onChangeMode;

        private int _currentIndex;

        public void SetText(int index)
        {
            _text.text = _options[index];
        }

        public void ClickPrevious()
        {
            SoundManager.Instance.PlayClickSound();
            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = _options.Count - 1;

            _text.text = _options[_currentIndex];
            onChangeMode?.Invoke(_currentIndex);
        }

        public void ClickNext()
        {
            SoundManager.Instance.PlayClickSound();
            _currentIndex++;
            if (_currentIndex >= _options.Count)
                _currentIndex = 0;
            
            _text.text = _options[_currentIndex];
            onChangeMode?.Invoke(_currentIndex);
        }
    }

    public enum SelectorMode
    {
        FullScreen,
        WindowedMaximized,
        Windowed
    }
    
    public enum SelectorResolution
    {
        HD,
        FullHD,
        FourK
    }
    public enum SelectorFPS
    {
        Thirty,
        Sixty,
        OneTwenty,
        OneFortyFour,
        TwoFortyFour
    }
}