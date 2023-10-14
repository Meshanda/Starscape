using System;
using System.Collections;
using System.Linq;
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
        
        [Header("Video")]
        [SerializeField] private Selector _fullscreenMode;
        [SerializeField] private Selector _resolution;
        [SerializeField] private Selector _frameRate;
        [SerializeField] private Toggle _vSync;
        
        private void Start()
        {
            Canvas.ForceUpdateCanvases(); 
            ApplyDefaultSettings();
            UpdateUI();
        }

        private void ApplyDefaultSettings()
        {
            SetFullscreenMode((int)Settings.fullScreenMode);
            SetResolution((int)Settings.resolution); 
            ToggleVSync(Settings.toggleVsync);
            SetFrameRate((int)Settings.targetFPS);
        }

        private void UpdateUI()
        {
            _sliderRed.value = Settings.cursorColor.r;
            _sliderGreen.value = Settings.cursorColor.g;
            _sliderBlue.value = Settings.cursorColor.b;

            _sliderSize.value = Settings.cursorSize;
            
            _sliderFx.value = Settings.volumeMusic;
            _sliderMusic.value = Settings.volumeFx;
            
            _fullscreenMode.SetText((int)Settings.fullScreenMode);
            _resolution.SetText((int)Settings.resolution);
            _frameRate.SetText((int)Settings.targetFPS);
            _vSync.isOn = Settings.toggleVsync;
        }

        public void ToggleVSync(bool status)
        {
            _frameRate.transform.parent.gameObject.SetActive(!status);
            Settings.toggleVsync = status;
            QualitySettings.vSyncCount = status ? 1 : 0;
            SoundManager.Instance.PlayClickSound();
        }
        
        public void SetFullscreenMode(int index)
        {
            switch ((SelectorMode) index)
            {
                case SelectorMode.FullScreen:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case SelectorMode.WindowedMaximized:
                    Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
                    break;
                case SelectorMode.Windowed:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            Settings.fullScreenMode = (SelectorMode)index;
        }
        public void SetResolution(int index)
        {
            switch ((SelectorResolution) index)
            {
                case SelectorResolution.HD:
                    Screen.SetResolution(1280, 720, Settings.IsFullScreen);
                    break;
                case SelectorResolution.FullHD:
                    Screen.SetResolution(1920, 1080, Settings.IsFullScreen);
                    break;
                case SelectorResolution.FourK:
                    Screen.SetResolution(7680, 4320, Settings.IsFullScreen);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }

            Settings.resolution = (SelectorResolution) index;
        }

        public void SetFrameRate(int index)
        {
            Application.targetFrameRate = (SelectorFPS) index switch
            {
                SelectorFPS.Thirty => 30,
                SelectorFPS.Sixty => 60,
                SelectorFPS.OneTwenty => 120,
                SelectorFPS.OneFortyFour => 144,
                SelectorFPS.TwoFortyFour => 244,
                _ => -1
            };

            Settings.targetFPS = (SelectorFPS) index;
        }

        public void VolumeFx(float value)
        {
            SoundManager.Instance.SetVolumeSfx(value);
        }
        
        public void VolumeMusic(float value)
        {
            SoundManager.Instance.SetVolumeMusic(value);
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