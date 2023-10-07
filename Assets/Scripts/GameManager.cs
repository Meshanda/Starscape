using System;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    [Header("References")]
    public DatabaseSO database;
    public Player player;
    [SerializeField] private Camera _minimapCamera;
    
    [Header("Menus")]
    [SerializeField] private GameObject _settingsCanvas;
    
    [Header("Pop ups")]
    [SerializeField] private GameObject _confirmerPopUp;
    [SerializeField] private GameObject _lostPopUp;
	

    public void Start()
    {
        SoundManager.OnGameStartMusic();
        
        CursorManager.Instance?.SetColor(Settings.cursorColor);
        CursorManager.Instance?.SetSize(Settings.cursorSize);

        InventorySystem.Instance.AddItem(new ItemStack{ itemID = "wood_pickaxe", number = 1});
        InventorySystem.Instance.AddItem(new ItemStack{ itemID = "wood_hammer", number = 1});
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
    
}