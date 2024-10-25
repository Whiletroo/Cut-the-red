using System;
using System.Collections.Generic;
using Cinemachine;
using UniSense;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombInput : MonoBehaviour
{
    public static BombInput Instance { get; private set; }

    private DualSenseGamepadHID _dualSense;
    public BombControls BombControls;
    public GameObject BombSlots;

    [SerializeField] private WireSlot _wireSlot;
    [SerializeField] private CinemachineVirtualCamera _mainVirtualCamera;

    private readonly List<GameObject> _listOfSlots = new();
    private readonly List<Outline> _listOfOutlines = new();

    private GameObject _currentObject;
    private BombSlot _currentSlot;
    private int _currentRow;
    private int _currentCol;
    private int _currentIndex;
    private int _previousIndex;

    private Action<InputAction.CallbackContext> _selectAction;
    public event EventHandler OnPause;
    public event EventHandler OnNavigate;

    private void Awake()
    {
        Instance = this;

        //BombControls
        BombControls = new BombControls();

        //DualSense
        _dualSense = DualSenseGamepadHID.FindCurrent();
        if (_dualSense == null) return;
        var triggerState = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.NoResistance
        };

        var state = new DualSenseGamepadState
        {
            PlayerLed = new PlayerLedState(false, false, false, false, false),
            LightBarColor = Color.cyan,
            LeftTrigger = triggerState,
            RightTrigger = triggerState
        };
        _dualSense.SetGamepadState(state);

        //Camera
        _mainVirtualCamera.enabled = true;

        //Callbacks
        BombControls.BombController.DPadUp.performed += ctx => HandleNavigate(DPadDirection.Up);
        BombControls.BombController.DPadDown.performed += ctx => HandleNavigate(DPadDirection.Down);
        BombControls.BombController.DPadLeft.performed += ctx => HandleNavigate(DPadDirection.Left);
        BombControls.BombController.DPadRight.performed += ctx => HandleNavigate(DPadDirection.Right);
        BombControls.FinalCut.ChooseWire.performed += ctx => HandleFinalCut();
        BombControls.CommonControlls.Pause.performed += ctx => PausePerformed();

        _selectAction = ctx => HandleSelect();

        // Outline
        foreach (Transform slot in BombSlots.transform)
        {
            _listOfSlots.Add(slot.gameObject);

            // Search for a child GameObject with the tag "SlotOutline"
            foreach (Transform child in slot)
            {
                if (child.CompareTag("SlotOutline"))
                {
                    // Get the Outline component of the child GameObject
                    var outline = child.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                        _listOfOutlines.Add(outline);
                        break; // Exit the loop once the outline object is found
                    }
                }
            }
        }

        // Set the first slot to be selected at the start of the game
        _listOfOutlines[0].enabled = true;
        _currentObject = _listOfSlots[0];

        _currentIndex = 0; // first slot is selected initially
        _previousIndex = 0; // previous index is also the first slot initially

        _currentSlot = _currentObject.GetComponent<BombSlot>();

        if (_currentSlot != null && _currentSlot.IsSelectable)
        {
            // Unsubscribe the previous select action
            if (_selectAction != null)
            {
                BombControls.BombController.Select.performed -= _selectAction;
            }

            // Create a new select action and subscribe it
            _selectAction = ctx => HandleSelect();
            BombControls.BombController.Select.performed += _selectAction;
        }
    }

    private void PausePerformed()
    {
        OnPause?.Invoke(this, EventArgs.Empty);
    }

    private void OnEnable()
    {
        BombControls.Enable();
        BombControls.FinalCut.Disable();
    }

    private void OnDisable()
    {
        BombControls.Disable();
    }

    enum DPadDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private void HandleNavigate(DPadDirection direction)
    {
        OnNavigate?.Invoke(this, EventArgs.Empty);

        switch (direction)
        {
            case DPadDirection.Up:
                _currentRow--;
                break;
            case DPadDirection.Down:
                _currentRow++;
                break;
            case DPadDirection.Left:
                _currentCol--;
                break;
            case DPadDirection.Right:
                _currentCol++;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }

        _currentRow = (_currentRow + 2) % 2;
        _currentCol = (_currentCol + 3) % 3;
        _currentIndex = _currentRow * 3 + _currentCol;

        if (_previousIndex >= 0 && _previousIndex < _listOfOutlines.Count)
        {
            _listOfOutlines[_previousIndex].enabled = false;
        }

        // Get current object
        _currentObject = _listOfSlots[_currentIndex];
        _listOfOutlines[_currentIndex].enabled = true;

        // Update the previousIndex to the current selectedIndex
        _previousIndex = _currentIndex;

        _currentSlot = _currentObject.GetComponent<BombSlot>();
        _listOfOutlines[_currentIndex].enabled = true;
    }

    private void HandleSelect()
    {
        if (_currentSlot.IsSelectable)
        {
            _currentSlot.HandleSlotControls(BombControls);
        }
    }

    private void HandleFinalCut()
    {
        if (_currentSlot.IsSelected) return;
        if (BombControls.FinalCut.enabled && !_wireSlot.WireIsSelected)
        {
            _wireSlot.HandleWireControls(BombControls);
        }
    }
}