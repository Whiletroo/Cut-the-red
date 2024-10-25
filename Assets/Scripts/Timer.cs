using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI TimerText;
    [SerializeField]
    private EType Type = EType.SecondsAndMinutes;

    public bool IsRunning { get; set; } = true;

    private enum EType
    {
        SecondsAndMinutes,
        OnlySeconds
    }

    public float TimeValue = 90;
    
    private void Update()
    {
        if (IsRunning && TimeValue > 0)
        {
            TimeValue -= Time.deltaTime;
        }

        DisplayTime(TimeValue);
    }

    private void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }

        if (Type == EType.OnlySeconds)
        {
            TimerText.text = $"{timeToDisplay:00}";
        }
        else
        {
            float minutes = Mathf.FloorToInt(timeToDisplay / 60);
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);

            TimerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    public string GetTime()
    {
        return TimerText.text;
    }
}
