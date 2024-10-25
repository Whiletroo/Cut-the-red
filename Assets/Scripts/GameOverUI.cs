using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [SerializeField]
    private GameObject _gameOverScreen;

    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    private List<Button> _buttons;
    private int _currentButtonIndex;

    private Transform _labelText;
    private Transform _descriptionText;
    private Transform _timeLeftText;

    private void Awake()
    {
        Instance = this;

        _labelText = _gameOverScreen.transform.Find("LabelText");
        _descriptionText = _gameOverScreen.transform.Find("DescriptionText");
        _timeLeftText = _gameOverScreen.transform.Find("TimeLeftText");

        _gameOverScreen.SetActive(false);

        _restartButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.GameScene);
        });

        _mainMenuButton.onClick.AddListener(() =>
        {
            Loader.Load(Loader.Scene.MainMenuScene);
        });

        _buttons = new List<Button> { _restartButton, _mainMenuButton };
        _currentButtonIndex = 0;

        EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);
    }

    public void ShowDetonateScreen()
    {
        _labelText.GetComponent<TMPro.TextMeshProUGUI>().text = "Game over";
        _descriptionText.GetComponent<TMPro.TextMeshProUGUI>().text = "The bomb exploded!";
        _timeLeftText.GetComponent<TMPro.TextMeshProUGUI>().text = "";

        _gameOverScreen.SetActive(true);

        _currentButtonIndex = 0;
        EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);
    }

    public void ShowDefuseScreen(string timeLeft)
    {
        _labelText.GetComponent<TMPro.TextMeshProUGUI>().text = "Congratulations";
        _descriptionText.GetComponent<TMPro.TextMeshProUGUI>().text = "You defused the bomb!";
        _timeLeftText.GetComponent<TMPro.TextMeshProUGUI>().text = "Time left: " + timeLeft;

        _gameOverScreen.SetActive(true);

        _currentButtonIndex = 0;
        EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);
    }

    private void HideGameOverScreen()
    {
        _gameOverScreen.SetActive(false);
    }
}
