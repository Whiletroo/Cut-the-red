using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] 
    private Button _playButton;
    [SerializeField] 
    private Button _quitButton;
    [SerializeField]
    private AudioClip _audioClip;

    private List<Button> _buttons;
    private int _currentButtonIndex;

    private MenuControls _menuControls;

    private enum NavigateDirection
    {
        Up,
        Down
    }

    private void Awake()
    {
        _menuControls = new MenuControls();

        _menuControls.MainMenu.NavigateUp.performed += ctx => HandleNavigate(NavigateDirection.Up);
        _menuControls.MainMenu.NavigateDown.performed += ctx => HandleNavigate(NavigateDirection.Down);
        _menuControls.MainMenu.Submit.performed += ctx => HandleSubmit();

        _playButton.onClick.AddListener(LoadScene);

        _quitButton.onClick.AddListener(Application.Quit);

        _buttons = new List<Button> { _playButton, _quitButton };
        _currentButtonIndex = 0;

        var outline = _playButton.GetComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.red;

        EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);
    }

    private void LoadScene()
    {
        Loader.Load(Loader.Scene.GameScene);
    }

    private void HandleSubmit()
    {
        var selectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (selectedButton != null)
        {
            selectedButton.onClick.Invoke();
        }
    }

    private void HandleNavigate(NavigateDirection direction)
    {
        AudioSource.PlayClipAtPoint(_audioClip, Camera.main.transform.position, 1f);

        switch (direction)
        {
            case NavigateDirection.Up:
            {
                _currentButtonIndex--;
                if (_currentButtonIndex < 0)
                {
                    _currentButtonIndex = _buttons.Count - 1;
                }

                break;
            }
            case NavigateDirection.Down:
            {
                _currentButtonIndex++;
                if (_currentButtonIndex >= _buttons.Count)
                {
                    _currentButtonIndex = 0;
                }

                break;
            }
        }

        foreach (var button in _buttons)
        {
            var outline = button.GetComponent<UnityEngine.UI.Outline>();
            if (button == _buttons[_currentButtonIndex])
            {
                outline.effectColor = Color.red;
            }
            else
            {
                outline.effectColor = Color.black;
            }
        }

        EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);
    }


    private void OnEnable()
    {
        _menuControls.MainMenu.Enable();
    }

    private void OnDisable()
    {
        _menuControls.MainMenu.Disable();
    }
}
