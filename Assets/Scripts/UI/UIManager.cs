using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static event Action OnPlayButtonClicked;
    public static event Action OnRestartButtonClicked;
    public static event Action OnPauseButtonClicked;
    public static event Action OnResumeButtonClicked;
    public static event Action OnHomePageButtonClicked;

    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject homePageButton;

    private void OnEnable()
    {
        playButton.SetActive(true);
        restartButton.SetActive(false);
        pauseButton.SetActive(false);
        resumeButton.SetActive(false);
        homePageButton.SetActive(false);
    }

    public void HandlePlayButtonClick()
    {
        playButton.SetActive(false);
        pauseButton.SetActive(true);
        restartButton.SetActive(true);
        
        OnPlayButtonClicked?.Invoke();
    }

    public void HandleRestartButtonClick()
    {
        OnRestartButtonClicked?.Invoke();
    }
    
    public void HandlePauseButtonClick()
    {
        pauseButton.SetActive(false);
        restartButton.SetActive(false);
        resumeButton.SetActive(true);
        homePageButton.SetActive(true);

        OnPauseButtonClicked?.Invoke();
    }

    public void HandleResumeButtonClick()
    {
        pauseButton.SetActive(true);
        restartButton.SetActive(true);
        resumeButton.SetActive(false);
        homePageButton.SetActive(false);
        
        OnResumeButtonClicked?.Invoke();
    }

    public void HandleHomePageButtonClick()
    {
        playButton.SetActive(true);
        resumeButton.SetActive(false);
        homePageButton.SetActive(false);
        
        OnHomePageButtonClicked?.Invoke();
    }
}
