using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioClipsRefsSO _audioClipsRefsSo;

    private Vector3 _wirePosition;
    private int _currentTickIndex;

    private void Start()
    {
        var wireSlot = WireSlot.Instance;
        _wirePosition = wireSlot.transform.position;

        BombInput.Instance.OnNavigate += BombInput_OnNavigate;
        BombSlot.OnAnySlotCompleted += BombSlot_OnAnySlotCompleted;
        WiresGame.Instance.OnWireCut += WiresGame_OnWireCut;
        WireSlot.Instance.BombWireCut += WireSlot_BombWireCut;
        BombManager.Instance.OnBombDefused += BombManager_OnBombDefused;
        BombManager.Instance.OnBombDetonated += BombManager_OnBombDetonated;
        BombManager.Instance.OnTimerTick += BombManager_OnTimerTick;
        BombManager.Instance.OnThirtySecondTick += BombManager_OnThirtySecondsTick;
        VentGame.Instance.OnTenSecondsPassed += VentGame_OnTenSecondsPassed;
    }

    private void OnDestroy()
    {
        BombInput.Instance.OnNavigate -= BombInput_OnNavigate;
        BombSlot.OnAnySlotCompleted -= BombSlot_OnAnySlotCompleted;
        WiresGame.Instance.OnWireCut -= WiresGame_OnWireCut;
        WireSlot.Instance.BombWireCut -= WireSlot_BombWireCut;
        BombManager.Instance.OnBombDefused -= BombManager_OnBombDefused;
        BombManager.Instance.OnBombDetonated -= BombManager_OnBombDetonated;
        BombManager.Instance.OnTimerTick -= BombManager_OnTimerTick;
        VentGame.Instance.OnTenSecondsPassed -= VentGame_OnTenSecondsPassed;
    }

    private void BombManager_OnThirtySecondsTick(object sender, System.EventArgs e)
    {
        PlaySound(_audioClipsRefsSo.ClockAlarm, _wirePosition, 1);
    }

    private void BombManager_OnTimerTick(object sender, System.EventArgs e)
    {

        PlaySound(_audioClipsRefsSo.ClockTick[_currentTickIndex], _wirePosition, 1);
        _currentTickIndex++;
        if (_currentTickIndex == _audioClipsRefsSo.ClockTick.Length)
        {
            _currentTickIndex = 0;
        }
    }

    private void VentGame_OnTenSecondsPassed(object sender, System.EventArgs e)
    {
        var ventGame = VentGame.Instance;
        PlaySound(_audioClipsRefsSo.VentBeep, ventGame.transform.position);
    }

    private void BombManager_OnBombDetonated(object sender, System.EventArgs e)
    {
        PlaySound(_audioClipsRefsSo.BigBang, Camera.main.transform.position);
    }

    private void BombManager_OnBombDefused(object sender, System.EventArgs e)
    {
        PlaySound(_audioClipsRefsSo.FaderSuccess, Camera.main.transform.position);
    }

    private void WireSlot_BombWireCut(object sender, System.EventArgs e)
    {
        var wireSlot = WireSlot.Instance;
        PlaySound(_audioClipsRefsSo.Defuse, wireSlot.transform.position, 1f);
    }

    private void WiresGame_OnWireCut(object sender, System.EventArgs e)
    {
        var wireGame = WiresGame.Instance;
        PlaySound(_audioClipsRefsSo.Defuse, wireGame.transform.position, 1f);
    }

    private void BombSlot_OnAnySlotCompleted(object sender, System.EventArgs e)
    {
        var bombSlot = sender as BombSlot;
        if (bombSlot != null) PlaySound(_audioClipsRefsSo.GreenLight, bombSlot.transform.position);
    }
    
    private void BombInput_OnNavigate(object sender, System.EventArgs e)
    {
        PlaySound(_audioClipsRefsSo.UiClick, Camera.main.transform.position);
    }

    private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volume = 0.7f)
    {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }

    private void PlaySound(AudioClip clip, Vector3 position, float volume = 0.7f)
    {
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}
