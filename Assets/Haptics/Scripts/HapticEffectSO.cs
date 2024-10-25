// Code is created by: Iain McManus and available at https://github.com/GameDevEducation/FeedbackTour/tree/Haptic-Feedback
// Modified by: Oleksandr Prokofiev
using UnityEngine;

[CreateAssetMenu(menuName = "Haptic Effect", fileName = "HapticEffect")]
public class HapticEffectSO : ScriptableObject
{
    public enum EType
    {
        OneShot,
        Continuous
    }

    public enum EGamepadPart
    {
        Left,
        Right,
        Both
    }

    [SerializeField] EType Type = EType.OneShot;
    [SerializeField] EGamepadPart GamepadPart = EGamepadPart.Both;

    [SerializeField] float Duration = 0f;

    [SerializeField] float LowSpeedIntensity = 1f;
    [SerializeField] AnimationCurve LowSpeedMotor;

    [SerializeField] float HighSpeedIntensity = 1f;
    [SerializeField] AnimationCurve HighSpeedMotor;

    [SerializeField] bool VariesWithDistance = false;
    [SerializeField] float MaxDistance = 25f;
    [SerializeField] AnimationCurve FallOffCurve;

    [System.NonSerialized] Vector3 EffectLocation;
    [System.NonSerialized] float Progress;
    public void Initialise(Vector3 _EffectLocation)
    {
        EffectLocation = _EffectLocation;
        Progress = 0f;
    }

    public bool Tick(Vector3 receiverPosition, out float lowSpeed, out float highSpeed)
    {
        // update the progress
        Progress += Time.deltaTime / Duration;

        // calculate the distance factor
        float distanceFactor = 1f;
        if (VariesWithDistance)
        {
            float distance = (receiverPosition - EffectLocation).magnitude;
            distanceFactor = distance >= MaxDistance ? 0f : FallOffCurve.Evaluate(distance / MaxDistance);
        }

        switch (GamepadPart)
        {
            case EGamepadPart.Left:
                lowSpeed = LowSpeedIntensity * distanceFactor * LowSpeedMotor.Evaluate(Progress);
                highSpeed = 0f;
                break;
            case EGamepadPart.Right:
                lowSpeed = 0f;
                highSpeed = HighSpeedIntensity * distanceFactor * HighSpeedMotor.Evaluate(Progress);
                break;
            case EGamepadPart.Both:
            default:
                lowSpeed = LowSpeedIntensity * distanceFactor * LowSpeedMotor.Evaluate(Progress);
                highSpeed = HighSpeedIntensity * distanceFactor * HighSpeedMotor.Evaluate(Progress);
                break;
        }
        
        // check if we are finished?
        if (Progress >= 1f)
        {
            if (Type == EType.OneShot)
                return true;

            Progress = 0f;
        }

        return false;
    }
}
