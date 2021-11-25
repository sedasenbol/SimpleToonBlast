using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameSceneLoaded;
    
    private GameState gameState = new GameState();

    private void OnEnable()
    {
        UIManager.OnPlayButtonClicked += StartGame;
        UIManager.OnRestartButtonClicked += RestartGame;
        UIManager.OnPauseButtonClicked += PauseGame;
        UIManager.OnResumeButtonClicked += ResumeGame;
        UIManager.OnHomePageButtonClicked += LoadHomePage;
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene != SceneManager.GetSceneByBuildIndex((int) GameState.Scene.Game)) {return;}

        SceneManager.SetActiveScene(scene);
        OnGameSceneLoaded?.Invoke();
    }

    private void StartGame()
    {
        SceneManager.LoadScene((int) GameState.Scene.Game, LoadSceneMode.Additive);
        
        gameState.CurrentScene = GameState.Scene.Game;
        gameState.CurrentState = GameState.State.OnPlay;
    }

    private void RestartGame()
    {
        SceneManager.UnloadSceneAsync((int) (GameState.Scene.Game));
        gameState = new GameState();
        
        StartGame();
    }
    
    private void PauseGame()
    {
        Time.timeScale = 0f;
        gameState.CurrentState = GameState.State.Paused;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        gameState.CurrentState = GameState.State.OnPlay;
    }

    private void LoadHomePage()
    {
        SceneManager.UnloadSceneAsync((int) (GameState.Scene.Game));
        Time.timeScale = 1f;

        gameState = new GameState(); 
    }

    private void OnDisable()
    {
        UIManager.OnPlayButtonClicked -= StartGame;
        UIManager.OnRestartButtonClicked -= RestartGame;
        UIManager.OnPauseButtonClicked -= PauseGame;
        UIManager.OnResumeButtonClicked -= ResumeGame;
        UIManager.OnHomePageButtonClicked -= LoadHomePage;
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
