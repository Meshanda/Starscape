
using System;

public class GameManager : Singleton<GameManager>
{
	public DatabaseSO database;
	public Player player;

    public void Start()
    {
        SoundManager.OnGameStartMusic();
    }

    private void OnEnable()
    {
        TimerSystem.TimerFinished += OnTimerFinished;
    }

    private void OnDisable()
    {
        TimerSystem.TimerFinished -= OnTimerFinished;
    }

    private void OnTimerFinished()
    {
        SceneLoader.LoadScene(GameState.EndScreen);
    }
}