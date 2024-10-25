using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class VentGame : BombSlot
{
    public new static VentGame Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _ventText;
    [SerializeField] private Timer _ventTimer;

    private BombManager _bombManager;
    private ETextOption _selectedTextOption;

    public event EventHandler OnTenSecondsPassed;

    private enum ETextOption
    {
        Defuse,
        Detonate
    }

    private enum SelectedButton
    {
        Yes,
        No
    }

    private bool _bombExploded;
    private float _lastEventTriggerTime;

    public override void Awake()
    {
        Instance = this;

        base.Awake();
        IsSelectable = true;

        _bombManager = FindObjectOfType<BombManager>();
        _selectedTextOption = (ETextOption)UnityEngine.Random.Range(0, 2);
        _ventText.text = _selectedTextOption == ETextOption.Defuse ? "Defuse the slot?" : "Detonate the bomb?";
    }

    public override void HandleSlotControls(BombControls bombControls)
    {
        base.HandleSlotControls(bombControls);

        SlotControls.VentGame.SelectYes.performed += HandleButtonSelectYes;
        SlotControls.VentGame.SelectNo.performed += HandleButtonSelectNo;
        SlotControls.VentGame.Enable();
    }

    private void HandleButtonSelectYes(InputAction.CallbackContext ctx) => HandleButtonSelect(SelectedButton.Yes);
    private void HandleButtonSelectNo(InputAction.CallbackContext ctx) => HandleButtonSelect(SelectedButton.No);

    private void HandleButtonSelect(SelectedButton selectedButton)
    {
        if (IsCompleted) return;
        if (_selectedTextOption == ETextOption.Defuse)
        {
            _ventTimer.TimeValue = 0;
            if (selectedButton == SelectedButton.Yes)
                SetCompleted(); // Defuse
            else
            {
                _bombManager.TriggerBombExplosion(); // Detonate
                _bombExploded = true;
            }
        }
        else
        {
            _ventTimer.TimeValue = 0;
            if (selectedButton == SelectedButton.Yes)
            {
                _bombManager.TriggerBombExplosion(); // Detonate
                _bombExploded = true;
            }
            else
                SetCompleted(); // Defuse
        }
    }

    private void Update()
    {
        if (!IsCompleted)
        {
            if (Mathf.FloorToInt(_ventTimer.TimeValue) % 10 == 0 && Time.time - _lastEventTriggerTime >= 10)
            {
                OnTenSecondsPassed?.Invoke(this, EventArgs.Empty);
                _lastEventTriggerTime = Time.time;
            }
        }


        if (_ventTimer.TimeValue <= 0 && !IsCompleted && !_bombExploded)
        {
            _bombManager.TriggerBombExplosion(); // Trigger bomb explosion if vent time runs out
            _bombExploded = true;
        }
    }

    public override void HandleReturn(InputAction.CallbackContext callbackContext)
    {
        SlotControls.VentGame.SelectYes.performed -= HandleButtonSelectYes;
        SlotControls.VentGame.SelectNo.performed -= HandleButtonSelectNo;
        SlotControls.VentGame.Disable();
        base.HandleReturn(default);
    }
}
