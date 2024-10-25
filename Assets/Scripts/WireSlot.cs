using System;
using UniSense;
using UnityEngine;
using UnityEngine.InputSystem;

public class WireSlot : MonoBehaviour
{
    public static WireSlot Instance { get; private set; }

    private DualSenseGamepadHID _dualSense;
    public BombControls WireControls;
    public CameraController CameraController;

    [SerializeField]
    private GameObject _cup;
    [SerializeField]
    private float _openRot, _closeRot, _speed;
    [SerializeField]
    private bool _opening;

    [SerializeField]
    private byte _startPosition;
    [SerializeField]
    private byte _endPosition;
    [SerializeField]
    private byte _force;

    [SerializeField]
    private GameObject _cutWireModel;
    [SerializeField]
    private GameObject _wireModel;

    public bool WireIsSelected;

    private bool _isOpened;
    private bool _bombDefused;
    private bool _triggerPulled;

    public event EventHandler BombWireCut;

    private void Awake()
    {
        Instance = this;

        CameraController = FindObjectOfType<CameraController>();
    }

    private void Update()
    {
        Quaternion currentRot = _cup.transform.localRotation;
        Quaternion targetRot;

        if (_opening)
        {
            targetRot = Quaternion.Euler(_openRot, currentRot.eulerAngles.y, currentRot.eulerAngles.z);
        }
        else
        {
            targetRot = Quaternion.Euler(_closeRot, currentRot.eulerAngles.y, currentRot.eulerAngles.z);
        }

        _cup.transform.localRotation = Quaternion.Lerp(currentRot, targetRot, Time.deltaTime * _speed);

        if (_dualSense != null && WireIsSelected)
        {
            var leftTriggerValue = Mathf.Lerp(0, _endPosition, _dualSense.leftTrigger.ReadValue());
            var rightTriggerValue = Mathf.Lerp(0, _endPosition, _dualSense.rightTrigger.ReadValue());

            if (!_triggerPulled)
            {
                if (!_bombDefused && leftTriggerValue >= _endPosition && rightTriggerValue >= _endPosition)
                {
                    // Deactivate the wire model and activate the cut wire model
                    _wireModel.SetActive(false);
                    _cutWireModel.SetActive(true);
                    _bombDefused = true;

                    BombWireCut?.Invoke(this, EventArgs.Empty);
                }
            }

            if (leftTriggerValue == 0 && rightTriggerValue == 0)
            {
                _triggerPulled = false;
            }
        }
    }


    private void ToggleDoor()
    {
        _opening = !_opening;
    }

    public void HandleWireControls(BombControls bombControls)
    {
        WireIsSelected = true;

        WireControls = bombControls;
        WireControls.BombController.Disable();
        CameraController.OnWire();

        WireControls.CommonControlls.Return.performed += ctx => HandleWireReturn(default);

        if (!_isOpened)
        {
            _isOpened = true;
            ToggleDoor();
        }
        
        _dualSense = DualSenseGamepadHID.FindCurrent();
        if (_dualSense == null) return;
        var triggerState = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.SectionResistance,
            Section =
            {
                StartPosition = _startPosition,
                EndPosition = _endPosition,
                Force = _force
            }
        };

        var state = new DualSenseGamepadState
        {
            LeftTrigger = triggerState,
            RightTrigger = triggerState
        };

        _dualSense.SetGamepadState(state);
    }
    
    private void HandleWireReturn(InputAction.CallbackContext callbackContext)
    {
        WireIsSelected = false;
        if (_isOpened)
        {
            _isOpened = false;
            ToggleDoor();
        }

        var triggerState = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.NoResistance,
        };

        var state = new DualSenseGamepadState
        {
            LeftTrigger = triggerState,
            RightTrigger = triggerState
        };
        _dualSense.SetGamepadState(state);

        WireControls.BombController.Enable();
        CameraController.OnReturn();

        WireControls.CommonControlls.Return.performed -= HandleWireReturn;
    }
}
