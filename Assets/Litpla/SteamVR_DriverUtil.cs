using System.IO;
using UnityEngine;

namespace Litpla.VR.Util
{
    public static class SteamVR_DriverUtil
    {
        public static steamvr_vrsettings ReadSteamVR_VRSettings()
        {
            var json = File.ReadAllText(Path.Combine(Application.dataPath, "steamvr.vrsettings"));
            return JsonUtility.FromJson<steamvr_vrsettings>(json);
        }

        public static default_vrsettings ReadDefault_VRSettings()
        {
            var json = File.ReadAllText(Path.Combine(Application.dataPath, "default.vrsettings"));
            return JsonUtility.FromJson<default_vrsettings>(json);
        }

        public static chaperone_info_vrchap ReadChaperone_Infol()
        {
            var json = File.ReadAllText(Path.Combine(Application.dataPath, "default.vrsettings"));
            return JsonUtility.FromJson<chaperone_info_vrchap>(json);
        }
    }
}