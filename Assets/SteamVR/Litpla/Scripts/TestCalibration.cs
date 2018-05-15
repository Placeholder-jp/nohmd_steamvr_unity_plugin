using HTC.UnityPlugin.Vive;
using Litpla.VR.Util;
using UnityEngine;

public class TestCalibration : MonoBehaviour
{
    [SerializeField] private KeyCode _key = KeyCode.C;
    [SerializeField] private VivePoseTracker _tracker;

    private void Update()
    {
        if (Input.GetKeyDown(_key))
            SteamVR_ChaperoneUtil.SetWorkingStandingZeroPoseFrom(_tracker);
    }
}