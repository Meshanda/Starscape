using System;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Canvases")] 
        [SerializeField] private GameObject _titleCanvas;
        [SerializeField] private GameObject _settingsCanvas;
        [SerializeField] private GameObject _creditsCanvas;
        
        [Header("Cameras")]
        [SerializeField] private GameObject _camTitle;
        [SerializeField] private GameObject _camSettings;
        [SerializeField] private GameObject _camCredits;

        [Header("Title Movement")]
        [SerializeField] private Transform _gameTitle;
        [SerializeField] private Vector3 _minScale;
        [SerializeField] private Vector3 _maxScale;
        [SerializeField] private float _scaleDelay;
        private Sequence _titleScaleSequence;
        
        [Header("Parallaxe")]
        [SerializeField] [Range(0,5)] private int moveModifier;

        private Vector3 _startPosCurrent;
        private Vector3 _startPosTitle;
        private Vector3 _startPosSettings;
        private Vector3 _startPosCredits;
        
        private Camera _mainCam;

        
        private void Start()
        {
            CursorManager.Instance?.SetColor(Settings.cursorColor);
            CursorManager.Instance?.SetSize(Settings.cursorSize);
            
            SoundManager.Instance.PlayMenuMusic();
            _startPosTitle = _camTitle.transform.localEulerAngles;
            _startPosSettings = _camSettings.transform.localEulerAngles;
            _startPosCredits = _camCredits.transform.localEulerAngles;

            _mainCam = Camera.main;
            
            ChangeCanvas(_titleCanvas);

            _titleScaleSequence = DOTween.Sequence();
            _titleScaleSequence.Append(_gameTitle.DOScale(_maxScale, _scaleDelay));
            _titleScaleSequence.Append(_gameTitle.DOScale(_minScale, _scaleDelay));
            _titleScaleSequence.SetEase(Ease.Linear);
            _titleScaleSequence.SetLoops(-1);
        }

        private void Update()
        {
            var pz = _mainCam.ScreenToViewportPoint(Input.mousePosition);
            pz = new Vector3(Mathf.Abs(pz.x), Mathf.Abs(pz.y), Mathf.Abs(pz.z));

            _startPosCurrent = GetStartPos();
            var posX = Mathf.Lerp(GetCurrentVCam().localEulerAngles.x, _startPosCurrent.x + (pz.x * moveModifier), 2f * Time.deltaTime);
            var posY = Mathf.Lerp(GetCurrentVCam().localEulerAngles.y, _startPosCurrent.y + (pz.y * moveModifier), 2f * Time.deltaTime);

            GetCurrentVCam().localEulerAngles = new Vector3(posX, posY, 0);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // To avoid alt tab bug.
            _startPosCurrent = Vector3.zero;
        }

        #region Click methods

        public void ClickPlay()
        {
            SoundManager.Instance.PlayClickSound();
            SceneLoader.LoadScene(GameState.GameScene);
        }
        public void ClickSettings()
        {
            SoundManager.Instance.PlayWhooshSound();
            SoundManager.Instance.PlayClickSound();
            ChangeCanvas(_settingsCanvas);
        }

        public void ClickCredits()
        {
            SoundManager.Instance.PlayWhooshSound();
            SoundManager.Instance.PlayClickSound();
            ChangeCanvas(_creditsCanvas);
        }

        public void ClickBackToTitle()
        {
            SoundManager.Instance.PlayWhooshSound();
            SoundManager.Instance.PlayClickSound();
            ChangeCanvas(_titleCanvas);
        }

        public void ClickQuit()
        {
            SoundManager.Instance.PlayClickSound();
#if UNITY_EDITOR
            if (Application.isEditor)
                EditorApplication.isPlaying = false;
            else
#endif
                Application.Quit();
        }

        #endregion

        #region Utils

        private void ChangeCanvas(GameObject canvas)
        {
            canvas.SetActive(true);
            
            if (canvas == _titleCanvas)
            {
                _settingsCanvas.SetActive(false);
                _creditsCanvas.SetActive(false);
                ChangeCam(_camTitle);
            }
            else if (canvas == _settingsCanvas)
            {
                _titleCanvas.SetActive(false);
                _creditsCanvas.SetActive(false);
                ChangeCam(_camSettings);
            }
            else if (canvas == _creditsCanvas)
            {
                _titleCanvas.SetActive(false);
                _settingsCanvas.SetActive(false);
                ChangeCam(_camCredits);
            }
        }
        
        private void ChangeCam(GameObject cam)
        {
            _camTitle.SetActive(false);
            _camSettings.SetActive(false);
            _camCredits.SetActive(false);

            cam.SetActive(true);
        }

        private Transform GetCurrentVCam()
        {
            if (_camTitle.activeSelf)
                return _camTitle.transform;
            if (_camSettings.activeSelf)
                return _camSettings.transform;
            if (_camCredits.activeSelf)
                return _camCredits.transform;

            throw new Exception("There are no active VCams in the scene!");
        }

        private Vector3 GetStartPos()
        {
            if (_camTitle.activeSelf)
                return _startPosTitle;
            if (_camSettings.activeSelf)
                return _startPosSettings;
            if (_camCredits.activeSelf)
                return _startPosCredits;

            throw new Exception("There are no active VCams in the scene!");
        }

        #endregion
        
    }
}