using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    GameScene,
    EndScreen
}

public static class SceneLoader
{
    private const string MAIN_MENU_SCENE = "MainMenu";
    private const string GAME_SCENE = "GameScene";
    private const string ENDSCREEN_SCENE = "EndScreen";

    public static GameState currentState = GameState.MainMenu;
    
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
            GameState.EndScreen => ENDSCREEN_SCENE,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}