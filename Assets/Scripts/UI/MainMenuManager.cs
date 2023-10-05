using System;
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
        
        [Header("Parallaxe")]
        [SerializeField] [Range(0,5)] private int moveModifier;
        private Vector3 _startPosTitle;
        private Vector3 _startPosSettings;
        private Vector3 _startPosCredits;
        
        private Camera _mainCam;
        private void Start()
        {
            _startPosTitle = _camTitle.transform.localEulerAngles;
            _startPosSettings = _camSettings.transform.localEulerAngles;
            _startPosCredits = _camCredits.transform.localEulerAngles;

            _mainCam = Camera.main;
            
            ChangeCanvas(_titleCanvas);
        }

        private void Update()
        {
            var pz = _mainCam.ScreenToViewportPoint(Input.mousePosition);

            var posX = Mathf.Lerp(GetCurrentVCam().localEulerAngles.x, GetStartPos().x + (pz.x * moveModifier), 2f * Time.deltaTime);
            var posY = Mathf.Lerp(GetCurrentVCam().localEulerAngles.y, GetStartPos().y + (pz.y * moveModifier), 2f * Time.deltaTime);

            GetCurrentVCam().localEulerAngles = new Vector3(posX, posY, 0);
        }

        #region Click methods

        public void ClickSettings()
        {
            ChangeCanvas(_settingsCanvas);
        }

        public void ClickCredits()
        {
            ChangeCanvas(_creditsCanvas);
        }

        public void ClickBackToTitle()
        {
            ChangeCanvas(_titleCanvas);
        }

        public void ClickQuit()
        {
            if (Application.isEditor)
                EditorApplication.isPlaying = false;
            else
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