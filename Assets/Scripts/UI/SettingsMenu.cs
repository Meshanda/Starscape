using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Cursor Color")] 
        [SerializeField] private Slider _sliderRed;
        [SerializeField] private Slider _sliderGreen;
        [SerializeField] private Slider _sliderBlue;

        [Header("Cursor Size")] 
        [SerializeField] private Slider _sliderSize;
        
        [Header("Volume")] 
        [SerializeField] private Slider _sliderFx;
        [SerializeField] private Slider _sliderMusic;

        private void Start()
        {
            _sliderRed.value = Settings.cursorColor.r;
            _sliderGreen.value = Settings.cursorColor.g;
            _sliderBlue.value = Settings.cursorColor.b;

            _sliderSize.value = Settings.cursorSize;
            
            _sliderFx.value = Settings.volumeMusic;
            _sliderMusic.value = Settings.volumeFx;
        }

        public void VolumeFx(float value)
        {
            
        }
        
        public void VolumeMusic(float value)
        {
            
        }

        public void CursorSize(float size)
        {
            CursorManager.Instance.SetSize(size);
        }
        
        public void CursorSliderR(float newR)
        {
            CursorManager.Instance.SetColor(new Color
            {
                r = newR,
                g = Settings.cursorColor.g,
                b = Settings.cursorColor.b,
                a = 1
            });
        }
        
        public void CursorSliderG(float newG)
        {
            CursorManager.Instance.SetColor(new Color
            {
                r = Settings.cursorColor.r,
                g = newG,
                b = Settings.cursorColor.b,
                a = 1
            }); 
        }
        
        public void CursorSliderB(float newB)
        {
            CursorManager.Instance.SetColor(new Color
            {
                r = Settings.cursorColor.r,
                g = Settings.cursorColor.g,
                b = newB,
                a = 1
            });
        }
    }
}