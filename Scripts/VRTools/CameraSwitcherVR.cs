using UnityEngine;
using UnityEngine.XR;

public class CameraSwitcherVR : MonoBehaviour
{
    public Camera vrCamera;
    public Camera normalCamera;

    private bool vrConnected;

    void Start()
    {
        CheckHeadset();
        ApplyCameraState();
    }

    void Update()
    {
        bool currentVRState = IsHeadsetConnected();
        if (currentVRState != vrConnected)
        {
            vrConnected = currentVRState;
            ApplyCameraState();
        }
    }

    private void CheckHeadset()
    {
        vrConnected = IsHeadsetConnected();
    }

    private bool IsHeadsetConnected()
    {
        var xrDevices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.Head, xrDevices);
        return xrDevices.Count > 0;
    }

    private void ApplyCameraState()
    {
        if (vrCamera == null || normalCamera == null) return;

        if (vrConnected)
        {
            vrCamera.gameObject.SetActive(true);
            normalCamera.gameObject.SetActive(false);
            vrCamera.targetDisplay = 0;
        }
        else
        {
            vrCamera.gameObject.SetActive(false);
            normalCamera.gameObject.SetActive(true);
            normalCamera.targetDisplay = 0;
        }
    }
}
