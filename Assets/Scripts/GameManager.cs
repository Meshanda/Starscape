using System;
using UI;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
	public DatabaseSO database;
	public Player player;

    [SerializeField] private Camera _minimapCamera;

    public void Start()
    {
        SoundManager.OnGameStartMusic();
        
        CursorManager.Instance?.SetColor(Settings.cursorColor);
        CursorManager.Instance?.SetSize(Settings.cursorSize);
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
        SceneLoader.LoadScene(GameState.EndScreen);
    }
}