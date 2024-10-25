using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UniSense;

public class TriggerGame : BombSlot
{
    public new static TriggerGame Instance { get; private set; }

    private DualSenseTriggerState _leftTriggerState;
    private DualSenseTriggerState _rightTriggerState;
    private DualSenseGamepadHID _dualSense;

    [SerializeField]
    private byte _forceleft;
    [SerializeField]
    private byte _forceright;

    [SerializeField]
    private float _leftMinGoal;
    [SerializeField]
    private float _leftMaxGoal;
    [SerializeField]
    private float _rightMinGoal;
    [SerializeField]
    private float _rightMaxGoal;

    [SerializeField]
    private float _targetHoldTime;
    private float _currentHoldTime;

    [SerializeField]
    private GameObject _leftBarPointer;
    [SerializeField]
    private GameObject _rightBarPointer;

    [SerializeField]
    private GameObject _leftBar;
    [SerializeField]
    private GameObject _rightBar;
    private float _leftBarHeight;
    private float _rightBarHeight;

    [SerializeField] 
    private AudioSource _triggerGoalSound;

    public override void Awake()
    {
        Instance = this;

        base.Awake();
        IsSelectable = true;
        
        // Get the bounds of the bar objects
        _leftBarHeight = _leftBar.GetComponent<MeshRenderer>().localBounds.size.z;
        _rightBarHeight = _rightBar.GetComponent<MeshRenderer>().localBounds.size.z;
    }

    public override void HandleSlotControls(BombControls bombControls)
    {
        base.HandleSlotControls(bombControls);

        _dualSense = DualSenseGamepadHID.FindCurrent();
        if (_dualSense == null) return;

        _leftTriggerState = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.ContinuousResistance,
            Continuous = new DualSenseContinuousResistanceProperties
            {
                StartPosition = (byte)(_leftMinGoal * 255),
                Force = _forceleft
            }
        };

        _rightTriggerState = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.ContinuousResistance,
            Continuous = new DualSenseContinuousResistanceProperties
            {
                StartPosition = (byte)(_rightMinGoal * 255),
                Force = _forceright
            }
        };

        var state = new DualSenseGamepadState
        {
            LeftTrigger = _leftTriggerState,
            RightTrigger = _rightTriggerState
        };
        _dualSense.SetGamepadState(state);

        StartCoroutine(UpdateTriggerGame());
    }

    private IEnumerator UpdateTriggerGame()
    {
        while (IsSelected && _dualSense != null)
        {
            var leftTriggerValue = Mathf.Clamp01(_dualSense.leftTrigger.ReadValue());
            var rightTriggerValue = Mathf.Clamp01(_dualSense.rightTrigger.ReadValue());

            // Map the trigger values to the range of pointer movement within the bars
            var leftPosition = Mathf.Lerp(0, _leftBarHeight, leftTriggerValue);
            var rightPosition = Mathf.Lerp(0, _rightBarHeight, rightTriggerValue);

            // Update the position of the pointers
            _leftBarPointer.transform.localPosition = new Vector3(_leftBarPointer.transform.localPosition.x,
                _leftBarPointer.transform.localPosition.y, leftPosition - _leftBarHeight / 2);
            _rightBarPointer.transform.localPosition = new Vector3(_rightBarPointer.transform.localPosition.x,
                _rightBarPointer.transform.localPosition.y, rightPosition - _rightBarHeight / 2);


            if (leftTriggerValue >= _leftMinGoal && leftTriggerValue <= _leftMaxGoal &&
                rightTriggerValue >= _rightMinGoal && rightTriggerValue <= _rightMaxGoal)
            {
                // If the sound is not playing, start it
                if (!_triggerGoalSound.isPlaying)
                {
                    _triggerGoalSound.Play();
                }

                _currentHoldTime += Time.deltaTime;
                if (_currentHoldTime >= _targetHoldTime)
                {
                    SetCompleted();
                    _currentHoldTime = 0.0f;
                }
            }
            else
            {
                // If the sound is playing, stop it
                if (_triggerGoalSound.isPlaying)
                {
                    _triggerGoalSound.Stop();
                }

                _currentHoldTime = 0.0f;
            }

            yield return null;
        }
    }
    
    public override void HandleReturn(InputAction.CallbackContext callbackContext)
    {
        var triggerState = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.NoResistance,
            Continuous = new DualSenseContinuousResistanceProperties
            {
                StartPosition = 0,
                Force = 0
            }
        };

        var state = new DualSenseGamepadState
        {
            LeftTrigger = triggerState,
            RightTrigger = triggerState
        };
        _dualSense.SetGamepadState(state);

        StopCoroutine(UpdateTriggerGame());
        base.HandleReturn(default);
    }
}
