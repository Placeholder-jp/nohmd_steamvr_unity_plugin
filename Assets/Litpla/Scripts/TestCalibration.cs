using Litpla.VR.Util;
using UnityEngine;

public class TestCalibration : MonoBehaviour
{
    [SerializeField] private KeyCode _key;
    [SerializeField] private SteamVR_TrackedObject _trackedObj;

    private void Update()
    {
        if (Input.GetKeyDown(_key))
            SteamVR_ChaperoneUtil.SetWorkingStandingZeroPoseFrom(_trackedObj);
    }
}