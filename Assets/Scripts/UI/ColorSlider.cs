using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ColorSlider : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Slider _slider;

        private void Start()
        {
            ChangeText(_slider.value);
        }

        public void ChangeText(float newValue)
        {
            _text.text = Math.Round(newValue, 2).ToString();
        }
    }
}