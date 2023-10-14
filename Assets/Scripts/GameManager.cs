using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("References")]
    public DatabaseSO database;
    public Player player;
    public Transform rocketTeleportSocket;
    [SerializeField] private Camera _minimapCamera;
    [SerializeField] private List<GameObject> _uiToDisableOnWin;
    
    [Header("Menus")]
    [SerializeField] private GameObject _settingsCanvas;
    
    [Header("Pop ups")]
    [SerializeField] private GameObject _confirmerPopUp;
    [SerializeField] private GameObject _lostPopUp;
    [SerializeField] private GameObject _rocketPopup;
    [SerializeField] private GameObject _wonPopup;

    public static event Action GameWon;

    public void Start()
    {
        UsableItem.ResetAllItems();
        SoundManager.OnGameStartMusic();
        
        CursorManager.Instance?.SetColor(Settings.cursorColor);
        CursorManager.Instance?.SetSize(Settings.cursorSize);

        InventorySystem.Instance.AddItem(new ItemStack{ itemID = "wood_pickaxe", number = 1});
        InventorySystem.Instance.AddItem(new ItemStack{ itemID = "wood_hammer", number = 1});
        InventorySystem.Instance.AddItem(new ItemStack{ itemID = "mirror", number = 1});
    }

    private void OnEnable()
    {
        TimerSystem.TimerFinished += OnTimerFinished;
        Minimap.ChangeMinimapZoom += OnChangeMinimapZoom;
    }

    private void OnDisable()
    {
        TimerSystem.TimerFinished -= OnTimerFinished;
        Minimap.ChangeMinimapZoom -= OnChangeMinimapZoom;
    }

    private void OnChangeMinimapZoom(int value)
    {
        _minimapCamera.orthographicSize = value;
    }

    private void OnTimerFinished()
    {
        SoundManager.Instance.PlayLoseMusic();
        TogglePopUpLost();
        Time.timeScale = 0;
    }

    public void ToggleSettings()
    {
        _settingsCanvas.SetActive(!_settingsCanvas.activeSelf);
        SoundManager.Instance.PlayClickSound();
    }
    
    public void TogglePopUpConfirmer()
    {
        _confirmerPopUp.SetActive(!_confirmerPopUp.activeSelf);
        SoundManager.Instance.PlayClickSound();
    }

    private void TogglePopUpLost()
    {
        _lostPopUp.SetActive(!_lostPopUp.activeSelf);
        SoundManager.Instance.PlayClickSound();
    }

    public void BackToMenu()
    {
        Time.timeScale = 1;
        SoundManager.Instance.PlayClickSound();
        SceneLoader.LoadScene(GameState.MainMenu);
    }

    public void ReloadGame()
    {
        Time.timeScale = 1;
        SoundManager.Instance.PlayClickSound();
        SceneLoader.LoadScene(GameState.GameScene);
    }
    
    public void LoseFocus()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ToggleRocketPopUp()
    {
        _rocketPopup.SetActive(!_rocketPopup.activeSelf);
    }

    public void Win()
    {
        SoundManager.Instance.PlayWinMusic();
        RenderSettings.skybox = null;
        ToggleRocketPopUp();
        _uiToDisableOnWin.ForEach(obj => obj.SetActive(false));
        GameWon?.Invoke();
    }

    public void ToggleWinScreen()
    {
        _wonPopup.SetActive(true);
    }
}