using System;
using UnityEngine;

public class TriggerExit : MonoBehaviour
{
    public static event Action BallExited;

    [SerializeField] private string _tagToCheck = "Ball";

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(_tagToCheck))
        {
            BallExited?.Invoke();
        }
    }
}
