using System;
using System.Collections.Generic;
using System.Linq;
using UniSense;
using UnityEngine;

public class BombManager : MonoBehaviour
{
    public static BombManager Instance { get; private set; }

    private List<BombSlot> _bombSlots;
    private int _completedSlots;
    private int _selectableSlots;
    private DualSenseGamepadHID _dualSense;

    [SerializeField] 
    private BombInput _bombInput;
    [SerializeField] 
    private SlotCounter _slotCounter;
    [SerializeField] 
    private Timer _bombTimer;

    private bool _bombExploded;
    private bool _isGamePaused;

    public event EventHandler OnGamePaused;
    public event EventHandler OnGameResumed;
    public event EventHandler OnBombDefused;
    public event EventHandler OnBombDetonated;
    public event EventHandler OnTimerTick;
    public event EventHandler OnThirtySecondTick;

    private float _timeElapsed;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _bombSlots = FindObjectsOfType<BombSlot>().ToList();

        _selectableSlots = _bombSlots.Count(slot => slot.IsSelectable);
        _completedSlots = _bombSlots.Count(slot => slot.IsCompleted && slot.IsSelectable);

        _slotCounter.ChangeProgress(_completedSlots, _selectableSlots);

        foreach (var bombSlot in _bombSlots)
        {
            bombSlot.OnSlotStateChanged += HandleBombCompletion;
        }

        WireSlot.Instance.BombWireCut += BombWireCut;
        BombInput.Instance.OnPause += BombInput_OnPause;

        ResumeGame();

        InvokeRepeating(nameof(TriggerTimerTick), 1f, 1f);
    }

    private void TriggerTimerTick()
    {
        // Trigger the TimerTick event every second
        OnTimerTick?.Invoke(this, EventArgs.Empty);
    }

    private void BombInput_OnPause(object sender, EventArgs e)
    {
        HandlePause();
    }
    
    private void Update()
    {
        _timeElapsed += Time.deltaTime;

        if (_timeElapsed >= 30f)
        {
            _timeElapsed = 0f;

            OnThirtySecondTick?.Invoke(this, EventArgs.Empty);
        }

        if (!_bombExploded && _bombTimer.TimeValue <= 0)
        {
            TriggerBombExplosion();
        }
    }

    public void TriggerBombExplosion()
    {
        _bombExploded = true;
        GameOver(EGameOverReason.Detonate);
    }

    private void BombWireCut(object sender, EventArgs e)
    {
        GameOver(EGameOverReason.Defuse);
    }

    private enum EGameOverReason
    {
        Defuse,
        Detonate
    }

    private void GameOver(EGameOverReason reason)
    {
        if (_bombInput != null)
        {
            _bombInput.BombControls.Disable();
        }

        switch (reason)
        {
            case EGameOverReason.Defuse:
                var timeLeft = _bombTimer.GetTime();
                PauseGame();
                OnBombDefused?.Invoke(this, EventArgs.Empty);
                GameOverUI.Instance.ShowDefuseScreen(timeLeft);
                break;
            case EGameOverReason.Detonate:
                PauseGame();
                OnBombDetonated?.Invoke(this, EventArgs.Empty);
                GameOverUI.Instance.ShowDetonateScreen();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
        }
    }

    private void HandleBombCompletion(object sender, System.EventArgs e)
    {
        _selectableSlots = _bombSlots.Count(slot => slot.IsSelectable);
        _completedSlots = _bombSlots.Count(slot => slot.IsCompleted && slot.IsSelectable);

        _slotCounter.ChangeProgress(_completedSlots, _selectableSlots);

        if (_completedSlots == _selectableSlots)
        {
            _bombInput.BombControls.FinalCut.Enable();
            _dualSense = DualSenseGamepadHID.FindCurrent();
            if (_dualSense == null) return;

            var state = new DualSenseGamepadState
            {
                PlayerLed = new PlayerLedState(true, true, true, true, true),
                LightBarColor = Color.green
            };
            _dualSense.SetGamepadState(state);
        }
    }

    public void HandlePause()
    {
        var wasFinalCutEnabledBeforePause = _bombInput.BombControls.FinalCut.enabled;
        var originalActionMap = _bombInput.BombControls.bindingMask;
        _isGamePaused = !_isGamePaused;
        if (_isGamePaused)
        {
            _bombInput.BombControls.Disable();
            _bombInput.BombControls.CommonControlls.Pause.Enable();
            OnGamePaused?.Invoke(this, EventArgs.Empty);
            PauseGame();
        }
        else
        {
            _bombInput.BombControls.Enable();
            if (!wasFinalCutEnabledBeforePause)
            {
                _bombInput.BombControls.FinalCut.Disable();
            }
            OnGameResumed?.Invoke(this, EventArgs.Empty);
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}
