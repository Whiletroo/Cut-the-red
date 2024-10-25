// Code is created by: Iain McManus and available at https://github.com/GameDevEducation/FeedbackTour/tree/Haptic-Feedback
// Modified by: Oleksandr Prokofiev
using System.Collections.Generic;
using UniSense;
using UnityEngine;

public class HapticManager : MonoBehaviour
{
    private DualSenseGamepadHID _dualSense;

    public static HapticManager Instance { get; private set; }

    List<HapticEffectSO> ActiveEffects = new();

    public static HapticEffectSO PlayEffect(HapticEffectSO effect, Vector3 location)
    {
        return Instance.PlayEffect_Internal(effect, location);
    }

    public static void StopEffect(HapticEffectSO effect)
    {
        Instance.StopEffect_Internal(effect);
    }

    private void Awake()
    {
        _dualSense = DualSenseGamepadHID.FindCurrent();

        if (Instance != null)
        {
            Debug.Log("Attempting to create second HapticManager on " + gameObject.name);

            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        var lowSpeedMotor = 0f;
        var highSpeedMotor = 0f;

        if (_dualSense != null)
        {
            for (int index = 0; index < ActiveEffects.Count; ++index)
            {
                var effect = ActiveEffects[index];

                // tick the effect and cleanup if finished
                float lowSpeedComponent = 0f;
                float highSpeedComponent = 0f;
                if (effect.Tick(Camera.main.transform.position, out lowSpeedComponent, out highSpeedComponent))
                {
                    ActiveEffects.RemoveAt(index);
                    --index;
                }

                // update the new speeds, constrain from 0 to 1
                lowSpeedMotor = Mathf.Clamp01(lowSpeedComponent + lowSpeedMotor);
                highSpeedMotor = Mathf.Clamp01(highSpeedComponent + highSpeedMotor);
            }

            // update the motors
            _dualSense.SetMotorSpeeds(lowSpeedMotor, highSpeedMotor);
        }
    }

    private void OnDestroy()
    {
        _dualSense.SetMotorSpeeds(0f, 0f);
    }

    private HapticEffectSO PlayEffect_Internal(HapticEffectSO effect, Vector3 location)
    {
        // setup the effect
        var activeEffect = Instantiate(effect);
        activeEffect.Initialise(location);

        ActiveEffects.Add(activeEffect);

        return activeEffect;
    }

    private void StopEffect_Internal(HapticEffectSO effect)
    {
        ActiveEffects.Remove(effect);
    }
}
