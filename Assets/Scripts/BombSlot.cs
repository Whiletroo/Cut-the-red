using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BombSlot : MonoBehaviour
{
    public static BombSlot Instance { get; private set; }

    public bool IsSelectable;
    public bool IsCompleted;
    public bool IsSelected;
    public int SlotIndex;
    public BombControls SlotControls;

    public event EventHandler OnSlotStateChanged;
    public static event EventHandler OnAnySlotCompleted;

    protected CameraController CameraController;
    [SerializeField] private GameObject _lamp = null;
    private MeshRenderer _lampRenderer;

    public virtual void Awake()
    {
        Instance = this;

        CameraController = FindObjectOfType<CameraController>();
        if (_lamp != null)
        {
            _lampRenderer = _lamp.GetComponent<MeshRenderer>();
            _lampRenderer.material = _lampRenderer.materials[0];
        }
    }

    public virtual void HandleSlotControls(BombControls bombControls)
    {
        IsSelected = true;
        SlotControls = bombControls;
        SlotControls.BombController.Disable();
        CameraController.OnSlotSelected(SlotIndex);

        SlotControls.CommonControlls.Return.performed -= HandleReturn;
        SlotControls.CommonControlls.Return.performed += ctx => HandleReturn(default);
    }

    public virtual void HandleReturn(InputAction.CallbackContext callbackContext)
    {
        var selectedWireSlot = FindObjectOfType<WireSlot>();
        if (selectedWireSlot.WireIsSelected) return;

        IsSelected = false;
        SlotControls.BombController.Enable();
        CameraController.OnReturn();

        SlotControls.CommonControlls.Return.performed -= HandleReturn;
    }

    public void SetCompleted()
    {
        IsCompleted = true;

        _lampRenderer.material = _lampRenderer.materials[1];
        OnSlotStateChanged?.Invoke(this, EventArgs.Empty);
        OnAnySlotCompleted?.Invoke(this, EventArgs.Empty);
    }
}