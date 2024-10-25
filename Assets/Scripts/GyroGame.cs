using System.Collections;
using System.Collections.Generic;
using UniSense;
using UnityEngine;
using UnityEngine.InputSystem;

public class GyroGame : BombSlot
{
    private DualSenseGamepadHID _dualSense;

    [SerializeField]
    private float _ballSpeed;
    [SerializeField]
    private Rigidbody _ball;
    [SerializeField]
    private Collider _field;
    [SerializeField]
    private Collider _exit;

    private Vector3 _localGyro;

    private List<RumbleZone> _rumbleZones = new();

    public override void Awake()
    {
        base.Awake();
        IsSelectable = true;

        // Get all RumbleZone components attached to child GameObjects
        _rumbleZones = new List<RumbleZone>(GetComponentsInChildren<RumbleZone>());
    }
    
    public override void HandleSlotControls(BombControls bombControls)
    {
        base.HandleSlotControls(bombControls);
        SlotControls.GyroGame.GyroX.Enable();
        SlotControls.GyroGame.GyroZ.Enable();
        _dualSense = DualSenseGamepadHID.FindCurrent();

        TriggerExit.BallExited += CompleteGame;

        // Allow physics on the ball when the slot is selected
        _ball.freezeRotation = false;

        StartCoroutine(UpdateGyroGame());
    }

    private IEnumerator UpdateGyroGame()
    {
        while (IsSelected && _dualSense != null)
        {
            var gyroX = SlotControls.GyroGame.GyroX.ReadValue<float>();
            var gyroZ = SlotControls.GyroGame.GyroZ.ReadValue<float>();

            // Form a movement vector
            _localGyro = new Vector3(gyroZ, 0, gyroX);

            yield return null; // wait for the next frame
        }
    }

    private void FixedUpdate()
    {
        // Calculate the normal of the plane
        var planeNormal = Vector3.Cross(_field.transform.right, _field.transform.forward).normalized;

        // Set the gravity direction to the negative of the plane's normal
        Physics.gravity = planeNormal * 9.81f;

        // Transform the gyroscopic input from the local space of the plane into world space
        var worldGyro = _field.transform.TransformDirection(_localGyro);

        // Rotate the movement direction to match the plane's rotation
        var rotatedMovement = Quaternion.Euler(_field.transform.eulerAngles) * worldGyro;

        var movement = rotatedMovement * _ballSpeed;
        _ball.AddForce(movement);
    }

    private void CompleteGame()
    {
        SetCompleted();  // Mark the game as completed
        _ball.gameObject.SetActive(false);
    }

    public override void HandleReturn(InputAction.CallbackContext callbackContext)
    {
        _ball.velocity = Vector3.zero;
        _ball.angularVelocity = Vector3.zero;
        _ball.freezeRotation = true;

        SlotControls.GyroGame.GyroX.Disable();
        SlotControls.GyroGame.GyroZ.Disable();
        TriggerExit.BallExited -= CompleteGame;

        // Stop the haptic effects for all rumble zones
        foreach (var rumbleZone in _rumbleZones)
        {
            var activeEffect = rumbleZone.GetActiveEffect();
            if (activeEffect != null)
            {
                HapticManager.StopEffect(activeEffect);
            }
        }

        base.HandleReturn(default);
        StopCoroutine(UpdateGyroGame());
    }
}
