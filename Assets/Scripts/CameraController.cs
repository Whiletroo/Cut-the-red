using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineVirtualCamera DefaultCamera;
    public CinemachineVirtualCamera WireCamera;
    public CinemachineVirtualCamera[] SlotCameras;

    private void Start()
    {
        // At start, make sure the main camera is active
        DefaultCamera.Priority = 15;
        WireCamera.Priority = 10;
        foreach (var camera in SlotCameras)
        {
            camera.Priority = 5;
        }
    }

    public void OnSlotSelected(int slotIndex)
    {
        // When a slot is selected, switch to the corresponding slot camera
        SlotCameras[slotIndex].Priority = 15;

        // Lower the priority of the other slot cameras and the main camera
        for (int i = 0; i < SlotCameras.Length; i++)
        {
            if (i != slotIndex)
            {
                SlotCameras[i].Priority = 5;
            }
        }
        DefaultCamera.Priority = 10;
        WireCamera.Priority = 5;
    }

    public void OnReturn()
    {
        // When returning, switch back to the main camera
        DefaultCamera.Priority = 15;

        WireCamera.Priority = 10;
        foreach (var t in SlotCameras)
        {
            t.Priority = 5;
        }
    }

    public void OnWire()
    {
        // When the wire is selected, switch to the wire camera
        WireCamera.Priority = 15;

        DefaultCamera.Priority = 10;
        foreach (var t in SlotCameras)
        {
            t.Priority = 5;
        }
    }
}
