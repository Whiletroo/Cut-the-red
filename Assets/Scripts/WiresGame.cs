using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniSense;
using UnityEngine;
using UnityEngine.InputSystem;

public class WiresGame : BombSlot
{
    public new static WiresGame Instance { get; private set; }

    private BombManager _bombManager;
    private DualSenseGamepadHID _dualSense;
    private DualSenseGamepadState _correctTriggerState;
    private DualSenseGamepadState _wrongTriggerState;
    private DualSenseGamepadState _noResistanceState;

    [SerializeField] private GameObject _wires;
    private List<GameObject> _wireList = new();
    private List<bool> _wireCutList = new();

    private List<(GameObject uncutModel, Material uncutNormalMaterial, Material uncutHighlightMaterial,
        GameObject cutModel, Material cutNormalMaterial, Material cutHighlightMaterial)> _wireModels = new();

    [SerializeField] private GameObject _strikes;
    private List<GameObject> _strikeList = new();

    [SerializeField] private float _correctTriggerStart;
    [SerializeField] private float _correctTriggerEnd;
    [SerializeField] private byte _correctTriggerForce;
    [SerializeField] private float _wrongTriggerStart;
    [SerializeField] private float _wrongTriggerEnd;
    [SerializeField] private byte _wrongTriggerForce;

    [SerializeField] private HapticEffectSO _correctEffect;
    [SerializeField] private HapticEffectSO _wrongEffect;

    private GameObject _correctWire;

    private int _currentIndex;
    private int _previousIndex;
    private bool _triggerPulled;

    public event EventHandler OnWireCut;


    private enum DPadDirection
    {
        Up,
        Down
    }

    public override void Awake()
    {
        Instance = this;

        base.Awake();
        IsSelectable = true;
        _bombManager = FindObjectOfType<BombManager>();

        ConfigureWireHighlight();
        GetCorrectWire();
        GetStrikeList();
    }

    private void GetStrikeList()
    {
        foreach (Transform strike in _strikes.transform)
        {
            _strikeList.Add(strike.gameObject);

            var strikeRenderer = strike.GetComponent<MeshRenderer>();
            strikeRenderer.material = strikeRenderer.materials[0];
        }
    }

    private void GetCorrectWire()
    {
        foreach (var wire in _wireList.Where(wire => wire.CompareTag("CorrectWire")))
        {
            _correctWire = wire;
        }
    }

    private void ConfigureWireHighlight()
    {
        foreach (Transform wire in _wires.transform)
        {
            _wireList.Add(wire.gameObject);
            _wireCutList.Add(false);

            var uncutModel = wire.Find("wire_model").gameObject;
            var cutModel = wire.Find("wire_cut").gameObject;

            var uncutRenderer = uncutModel.GetComponent<MeshRenderer>();
            var cutRenderer = cutModel.GetComponent<MeshRenderer>();

            _wireModels.Add((uncutModel, uncutRenderer.materials[0], uncutRenderer.materials[1], cutModel,
                cutRenderer.materials[0], cutRenderer.materials[1]));
        }
    }

    private void SwitchWireModel(int index, bool cut)
    {
        var (uncutModel, uncutNormalMaterial, uncutHighlightMaterial, cutModel, cutNormalMaterial, cutHighlightMaterial
            ) = _wireModels[index];

        // Switch the models
        uncutModel.SetActive(!cut);
        cutModel.SetActive(cut);

        // Adjust the material for both models
        var uncutRenderer = uncutModel.GetComponent<MeshRenderer>();
        uncutRenderer.material = cut ? uncutNormalMaterial : uncutHighlightMaterial;

        var cutRenderer = cutModel.GetComponent<MeshRenderer>();
        cutRenderer.material = cut ? cutHighlightMaterial : cutNormalMaterial;
    }

    private void SetFail()
    {
        var strikeCount = _strikeList.Count(strike =>
            strike.GetComponent<MeshRenderer>().material == strike.GetComponent<MeshRenderer>().materials[1]);

        if (strikeCount < 2)
        {
            var currentStrike = _strikeList[strikeCount];
            var strikeRenderer = currentStrike.GetComponent<MeshRenderer>();
            strikeRenderer.material = strikeRenderer.materials[1];
        }
        else
        {
            _bombManager.TriggerBombExplosion();
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            if (_dualSense == null || IsCompleted || _wireCutList[_currentIndex])
            {
                yield return null;
                continue;
            }

            HandleTriggerPull();

            yield return null;
        }
    }

    private void HandleTriggerPull()
    {
        var selectedWire = _wireList[_currentIndex];
        var leftTriggerValue = Mathf.Clamp01(_dualSense.leftTrigger.ReadValue());
        var rightTriggerValue = Mathf.Clamp01(_dualSense.rightTrigger.ReadValue());

        if (!_triggerPulled)
        {
            if (selectedWire == _correctWire && leftTriggerValue >= _correctTriggerEnd &&
                rightTriggerValue >= _correctTriggerEnd)
            {
                SetCompleted();
                _triggerPulled = true;
                _wireCutList[_currentIndex] = true;
                OnWireCut?.Invoke(this, EventArgs.Empty);
                SwitchWireModel(_currentIndex, true);
            }
            else if (selectedWire != _correctWire && leftTriggerValue >= _wrongTriggerEnd &&
                     rightTriggerValue >= _wrongTriggerEnd)
            {
                SetFail();
                _triggerPulled = true;
                _wireCutList[_currentIndex] = true;
                OnWireCut?.Invoke(this, EventArgs.Empty);
                SwitchWireModel(_currentIndex, true);
            }
        }

        if (leftTriggerValue == 0 && rightTriggerValue == 0)
        {
            _triggerPulled = false;
        }
    }

    private void HandleNavigate(DPadDirection direction)
    {
        // Handle material for previous wire
        if (_previousIndex >= 0 && _previousIndex < _wireModels.Count)
        {
            var previousWire = _wireModels[_previousIndex];
            if (_wireCutList[_previousIndex])
            {
                var previousCutRenderer = previousWire.cutModel.GetComponent<MeshRenderer>();
                previousCutRenderer.material = previousWire.cutNormalMaterial;
            }
            else
            {
                var previousRenderer = previousWire.uncutModel.GetComponent<MeshRenderer>();
                previousRenderer.material = previousWire.uncutNormalMaterial;
            }
        }

        switch (direction)
        {
            case DPadDirection.Up:
                _currentIndex--;
                break;
            case DPadDirection.Down:
                _currentIndex++;
                break;
        }

        // Make sure the index is within the bounds
        _currentIndex = Mathf.Clamp(_currentIndex, 0, _wireList.Count - 1);

        var selectedWire = _wireList[_currentIndex];

        // Check if the selected wire is the correct one
        if (_wireCutList[_currentIndex])
        {
            _dualSense.SetGamepadState(_noResistanceState);
        }
        else if (selectedWire == _correctWire)
        {
            HapticManager.PlayEffect(_correctEffect, transform.position);
            _dualSense.SetGamepadState(_correctTriggerState);
        }
        else
        {
            HapticManager.PlayEffect(_wrongEffect, transform.position);
            _dualSense.SetGamepadState(_wrongTriggerState);
        }

        var selectedWireModel = _wireModels[_currentIndex];
        var selectedCutRenderer = selectedWireModel.cutModel.GetComponent<MeshRenderer>();
        var selectedRenderer = selectedWireModel.uncutModel.GetComponent<MeshRenderer>();

        if (_wireCutList[_currentIndex])
        {
            selectedCutRenderer.material = selectedWireModel.cutHighlightMaterial;
        }
        else
        {
            selectedRenderer.material = selectedWireModel.uncutHighlightMaterial;
        }

        _previousIndex = _currentIndex;
    }

    public override void HandleSlotControls(BombControls bombControls)
    {
        base.HandleSlotControls(bombControls);

        var currentWireModel = _wireModels[_currentIndex];
        if (_wireCutList[_currentIndex])
        {
            var currentRenderer = currentWireModel.cutModel.GetComponent<MeshRenderer>();
            currentRenderer.material = currentWireModel.cutHighlightMaterial;
        }
        else
        {
            var currentRenderer = currentWireModel.uncutModel.GetComponent<MeshRenderer>();
            currentRenderer.material = currentWireModel.uncutHighlightMaterial;
        }

        _dualSense = DualSenseGamepadHID.FindCurrent();
        if (_dualSense == null) return;

        var correctTrigger = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.SectionResistance,
            Section = new DualSenseSectionResistanceProperties()
            {
                StartPosition = (byte)(_correctTriggerStart * 255),
                EndPosition = (byte)(_correctTriggerEnd * 255),
                Force = _correctTriggerForce
            }
        };

        var wrongTrigger = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.SectionResistance,
            Section = new DualSenseSectionResistanceProperties()
            {
                StartPosition = (byte)(_wrongTriggerStart * 255),
                EndPosition = (byte)(_wrongTriggerEnd * 255),
                Force = _wrongTriggerForce
            }
        };

        var noResistance = new DualSenseTriggerState
        {
            EffectType = DualSenseTriggerEffectType.NoResistance,
            Continuous = new DualSenseContinuousResistanceProperties
            {
                StartPosition = 0,
                Force = 0
            }
        };

        _noResistanceState = new DualSenseGamepadState
        {
            LeftTrigger = noResistance,
            RightTrigger = noResistance
        };

        _correctTriggerState = new DualSenseGamepadState
        {
            LeftTrigger = correctTrigger,
            RightTrigger = correctTrigger
        };

        _wrongTriggerState = new DualSenseGamepadState
        {
            LeftTrigger = wrongTrigger,
            RightTrigger = wrongTrigger
        };

        _dualSense.SetGamepadState(_wrongTriggerState);

        SlotControls.WiresGame.Enable();

        SlotControls.WiresGame.DPadUp.performed += ctx => HandleNavigate(DPadDirection.Up);
        SlotControls.WiresGame.DPadDown.performed += ctx => HandleNavigate(DPadDirection.Down);

        StartCoroutine(UpdateCoroutine());
    }

    public override void HandleReturn(InputAction.CallbackContext callbackContext)
    {
        _dualSense.SetGamepadState(_noResistanceState);

        var currentWireModel = _wireModels[_currentIndex];
        if (_wireCutList[_currentIndex])
        {
            var currentRenderer = currentWireModel.cutModel.GetComponent<MeshRenderer>();
            currentRenderer.material = currentWireModel.cutNormalMaterial;
        }
        else
        {
            var currentRenderer = currentWireModel.uncutModel.GetComponent<MeshRenderer>();
            currentRenderer.material = currentWireModel.uncutNormalMaterial;
        }

        SlotControls.WiresGame.Disable();
        StopCoroutine(UpdateCoroutine());
        base.HandleReturn(default);
    }
}
