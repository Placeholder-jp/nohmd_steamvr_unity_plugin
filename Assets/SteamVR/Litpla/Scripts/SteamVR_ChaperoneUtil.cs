using System.Collections;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace Litpla.VR.Util
{
    /// <summary>
    /// ランタイムで任意のTrackedObjectを使いルームセットアップを行うためのクラス
    /// </summary>
    public static class SteamVR_ChaperoneUtil
    {
        private static readonly float CentimetersToMeters = 0.01f;
        private static readonly float InchesToMeters = 0.0254f;
        private static Vector3 calibratedCenterPosition = Vector3.zero;
        private static Quaternion calibratedCenterRotation = Quaternion.identity;
        private static float calibratedFloorPosition = 0f;
        private static float heightOffset = 0f;
        private static readonly bool heightOffsetInInchesNotCentimeters = false;


        /// <summary>
        /// 与えられたTrackedObjectの位置・回転を基準に立位のルームセットアップを行います
        /// </summary>
        /// <param name="trackedObj"></param>
        public static void SetWorkingStandingZeroPoseFrom(SteamVR_TrackedObject trackedObj)
        {
            if (trackedObj != null)
                trackedObj.StartCoroutine(CalibrateAfterResetChaperoneInfo(trackedObj));
        }

        private static IEnumerator CalibrateAfterResetChaperoneInfo(SteamVR_TrackedObject trackedObj)
        {
            SteamVR_MonitorUtil.DeleteChaperone_Info();
            OpenVR.ChaperoneSetup.ReloadFromDisk(EChaperoneConfigFile.Live);
            yield return null;
            var deviceIdx = trackedObj.index;
            var device = SteamVR_Controller.Input((int)deviceIdx);
            var trans = new SteamVR_Utils.RigidTransform(device.GetPose().mDeviceToAbsoluteTracking);
            calibratedCenterPosition = trans.pos;
            calibratedCenterRotation = Quaternion.Euler(trans.rot.eulerAngles.x, trans.rot.eulerAngles.y, 0f);
            calibratedFloorPosition = trans.pos.y - 0.05f;
            Calibrate(calibratedCenterPosition, calibratedCenterRotation, calibratedFloorPosition);
        }

        private static void Calibrate(Vector3 calibratedCenterPosition, Quaternion calibratedCenterRotation,
            float calibratedFloorPosition)
        {
            Vector3 zeroPosePosition;
            Quaternion zeroPoseRotation;
            Vector2[] hardBoundsPoints;
            Vector2[] softBoundsPoints;
            GetStandingOnlyConfigurationValues(out zeroPosePosition, out zeroPoseRotation, out hardBoundsPoints,
                out softBoundsPoints);
            var chaperoneSetup = OpenVR.ChaperoneSetup;
            if (chaperoneSetup == null)
            {
                Debug.LogError("Failed to get chaperone setup interface.");
            }
            else
            {
                var rigidTransform = new SteamVR_Utils.RigidTransform(zeroPosePosition, zeroPoseRotation);
                var hmdMatrix34 = rigidTransform.ToHmdMatrix34();

                chaperoneSetup.SetWorkingStandingZeroPoseToRawTrackingPose(ref hmdMatrix34);
                var pQuadsBuffer = new HmdQuad_t[hardBoundsPoints.Length];
                for (var index1 = 0; index1 < pQuadsBuffer.Length; ++index1)
                {
                    var index2 = (index1 + 1) % pQuadsBuffer.Length;
                    var point = new Vector3(hardBoundsPoints[index1].x, 0.0f, hardBoundsPoints[index1].y);
                    var vector3 = rigidTransform.InverseTransformPoint(point);
                    pQuadsBuffer[index1].vCorners0.v0 = vector3.x;
                    pQuadsBuffer[index1].vCorners0.v1 = 0.0f;
                    pQuadsBuffer[index1].vCorners0.v2 = -vector3.z;
                    pQuadsBuffer[index1].vCorners1.v0 = vector3.x;
                    pQuadsBuffer[index1].vCorners1.v1 = 2.43f;
                    pQuadsBuffer[index1].vCorners1.v2 = -vector3.z;
                    point = new Vector3(hardBoundsPoints[index2].x, 0.0f, hardBoundsPoints[index2].y);
                    vector3 = rigidTransform.InverseTransformPoint(point);
                    pQuadsBuffer[index1].vCorners2.v0 = vector3.x;
                    pQuadsBuffer[index1].vCorners2.v1 = 2.43f;
                    pQuadsBuffer[index1].vCorners2.v2 = -vector3.z;
                    pQuadsBuffer[index1].vCorners3.v0 = vector3.x;
                    pQuadsBuffer[index1].vCorners3.v1 = 0.0f;
                    pQuadsBuffer[index1].vCorners3.v2 = -vector3.z;
                }
                chaperoneSetup.SetWorkingCollisionBoundsInfo(pQuadsBuffer);
                var a1 = 0.0f;
                var a2 = 0.0f;
                var a3 = 0.0f;
                var a4 = 0.0f;
                for (var index = 0; index < softBoundsPoints.Length; ++index)
                {
                    var point = new Vector3(softBoundsPoints[index].x, 0.0f, softBoundsPoints[index].y);
                    var vector3 = rigidTransform.InverseTransformPoint(point);
                    a1 = Mathf.Min(a1, vector3.x);
                    a2 = Mathf.Max(a2, vector3.x);
                    a3 = Mathf.Min(a3, vector3.z);
                    a4 = Mathf.Max(a4, vector3.z);
                }
                chaperoneSetup.SetWorkingPlayAreaSize(a2 - a1, a4 - a3);
                chaperoneSetup.CommitWorkingCopy(EChaperoneConfigFile.Live);
                chaperoneSetup.ReloadFromDisk(EChaperoneConfigFile.Live);
                Debug.Log("Chaperone setup completed");
            }
        }

        private static void GetStandingOnlyConfigurationValues(out Vector3 zeroPosePosition,
            out Quaternion zeroPoseRotation,
            out Vector2[] hardBoundsPoints, out Vector2[] softBoundsPoints)
        {
            var y = calibratedFloorPosition -
                    heightOffset * (!heightOffsetInInchesNotCentimeters ? CentimetersToMeters : InchesToMeters);
            zeroPosePosition = new Vector3(calibratedCenterPosition.x, y, calibratedCenterPosition.z);
            var forward = calibratedCenterRotation * Vector3.forward;
            forward.y = 0.0f;
            forward.Normalize();
            zeroPoseRotation = Quaternion.LookRotation(forward);
            var vector2Array = NormalizePolygonWinding(new Vector3[4]
            {
                zeroPosePosition + zeroPoseRotation * new Vector3(0.5f, 0.0f, 0.5f),
                zeroPosePosition + zeroPoseRotation * new Vector3(0.5f, 0.0f, -0.5f),
                zeroPosePosition + zeroPoseRotation * new Vector3(-0.5f, 0.0f, -0.5f),
                zeroPosePosition + zeroPoseRotation * new Vector3(-0.5f, 0.0f, 0.5f)
            }.Select(v => new Vector2(v.x, v.z)).ToArray());
            hardBoundsPoints = vector2Array;
            softBoundsPoints = vector2Array;
        }

        private static Vector2[] NormalizePolygonWinding(Vector2[] polygon)
        {
            var num = 0.0f;
            for (var index = 0; index < polygon.Length; ++index)
            {
                var vector2_1 = polygon[index];
                var vector2_2 = polygon[(index + 1) % polygon.Length];
                num += (float)((vector2_2.x - (double)vector2_1.x) * (vector2_2.y + (double)vector2_1.y));
            }
            if (num < 0.0)
                return polygon;
            return polygon.Reverse().ToArray();
        }
    }
}