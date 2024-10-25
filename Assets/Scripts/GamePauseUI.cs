using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class GamePauseUI : MonoBehaviour
{
    public static GamePauseUI Instance { get; private set; }
    
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _mainMenuButton;

    private List<Button> _buttons;
    private int _currentButtonIndex;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BombManager.Instance.OnGamePaused += OnGamePaused;
        BombManager.Instance.OnGameResumed += OnGameResumed;

        _resumeButton.onClick.AddListener(() =>
        {
            BombManager.Instance.HandlePause();
        });

        _mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        _buttons = new List<Button> { _resumeButton, _mainMenuButton };
        _currentButtonIndex = 0;

        EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);

        Hide();
    }

    private void OnGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void OnGameResumed(object sender, EventArgs e)
    {
       Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
