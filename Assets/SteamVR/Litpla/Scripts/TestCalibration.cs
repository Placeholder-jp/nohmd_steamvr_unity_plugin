using System.Collections;
using HTC.UnityPlugin.Vive;
using Litpla.VR.Util;
using UnityEngine;
using Valve.VR;

public class TestCalibration : MonoBehaviour
{
    [SerializeField] private KeyCode _key = KeyCode.C;
    [SerializeField] private Transform _target;

    private void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            StartCoroutine(Calibrate());
        }
    }

    IEnumerator Calibrate()
    {
        SteamVR_ChaperoneUtil.Reset();

        yield return null;

        SteamVR_ChaperoneUtil.SetWorkingStandingZeroPoseFrom(_target.position, _target.rotation);
    }
}