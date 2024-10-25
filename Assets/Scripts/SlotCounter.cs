using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniSense;
using UnityEngine;

public class SlotCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _timerText;

    public void ChangeProgress(int completed, int overall)
    {
        _timerText.text = $"{completed:0}/{overall:0}";
    }
}
