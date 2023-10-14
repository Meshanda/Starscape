using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    GameScene,
}

public static class SceneLoader
{
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string GAME_SCENE = "GameScene";

    public static GameState currentState = GameState.MainMenu; // TODO CHECK

    public static void LoadScene(GameState newState, Action<AsyncOperation> callback = null)
    {
        SceneManager.LoadSceneAsync(GetSceneNameWithState(newState)).completed += callback;
        currentState = newState;
    }
    
    private static string GetSceneNameWithState(GameState state)
    {
        return state switch
        {
            GameState.MainMenu => MAIN_MENU_SCENE,
            GameState.GameScene => GAME_SCENE,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}