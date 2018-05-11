using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Litpla.VR.Util
{
    public static class SteamVR_DriverUtil
    {
        public static bool IsRestarting { get; private set; }

        private static string DefaultSteamPath
        {
            get
            {
                var path = string.Empty;
#if NET_4_6
                path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                path += "(x86)";
#else
                path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                path += " (x86)";
#endif
                path = Path.Combine(path, "Steam");
                return path;
            }
        }

        private static string CustomSteamPath
        {
            get { return string.Empty; }
        }

        private static string SteamPath
        {
            get { return string.IsNullOrEmpty(CustomSteamPath) ? DefaultSteamPath : CustomSteamPath; }
        }

        private static string steamvr_vrsettings_path
        {
            get { return Path.Combine(SteamPath, "config/steamvr.vrsettings"); }
        }

        private static string chaperone_info_vrchap_path
        {
            get { return Path.Combine(SteamPath, "config/chaperone_info.vrchap"); }
        }

        private static string default_vrsettings_path
        {
            get
            {
                return Path.Combine(SteamPath,
                    "steamapps/common/SteamVR/drivers/null/resources/settings/default.vrsettings");
            }
        }

        public static string vrstartup_exe_path
        {
            get { return Path.Combine(SteamPath, "steamapps/common/SteamVR/bin/win64/vrstartup.exe"); }
        }


        private static steamvr_vrsettings ReadSteamVR_VRSettings()
        {
            var data = new steamvr_vrsettings();
            var path = steamvr_vrsettings_path;
            if (File.Exists(path))
            {
                var fileJson = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(fileJson, data);
            }
            return data;
        }

        private static chaperone_info_vrchap ReadChaperone_Info_VRChap()
        {
            var data = new chaperone_info_vrchap();
            var path = chaperone_info_vrchap_path;
            if (File.Exists(path))
            {
                var fileJson = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(fileJson, data);
            }
            return data;
        }

        private static default_vrsettings ReadDefault_VRSettings()
        {
            var data = new default_vrsettings();
            var path = default_vrsettings_path;
            if (File.Exists(path))
            {
                var fileJson = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(fileJson, data);
            }
            return data;
        }

        private static void WriteSteamVR_VRSettings(steamvr_vrsettings data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = steamvr_vrsettings_path;
            File.WriteAllText(path, json);
        }

        private static void WriteChaperone_Info_VRChap(chaperone_info_vrchap data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = chaperone_info_vrchap_path;
            File.WriteAllText(path, json);
        }

        private static void WriteDefault_VRSettings(default_vrsettings data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = default_vrsettings_path;
            File.WriteAllText(path, json);
        }

        public static void ActivateNoRequiredHMDSettings()
        {
            var s = ReadSteamVR_VRSettings();
            s.steamvr.activateMultipleDrivers = true;
            s.steamvr.forcedDriver = "null";
            WriteSteamVR_VRSettings(s);

            var d = ReadDefault_VRSettings();
            d.driver_null.enable = true;
            d.driver_null.displayFrequency = 60f;
            WriteDefault_VRSettings(d);

            var c = ReadChaperone_Info_VRChap();
            WriteChaperone_Info_VRChap(c);

            RestartSteamVR();
        }

        public static void DeactivateNoRequiredHMDSettings()
        {
            var s = ReadSteamVR_VRSettings();
            s.steamvr.activateMultipleDrivers = false;
            s.steamvr.forcedDriver = string.Empty;
            WriteSteamVR_VRSettings(s);

            var d = ReadDefault_VRSettings();
            d.driver_null.enable = false;
            d.driver_null.displayFrequency = 90f;
            WriteDefault_VRSettings(d);

            RestartSteamVR();
        }

        public static void RestartSteamVR()
        {
            if (IsRestarting)
                return;

            var thread = new Thread(RestartThread);
            thread.Start();
            IsRestarting = true;
        }

        private static void RestartThread()
        {
            Debug.Log("Restarting SteamVR...");
            var processExists = false;
            foreach (var process in Process.GetProcessesByName("vrmonitor"))
            {
                processExists = true;
                process.CloseMainWindow();
                while (!process.HasExited)
                    Thread.Sleep(500);
            }

            foreach (var process in Process.GetProcessesByName("vrcompositor"))
            {
                processExists = true;
                process.CloseMainWindow();
                while (!process.HasExited)
                    Thread.Sleep(500);
            }

            foreach (var process in Process.GetProcessesByName("vrserver"))
            {
                processExists = true;
                process.CloseMainWindow();
                while (!process.HasExited)
                    Thread.Sleep(500);
            }

            foreach (var process in Process.GetProcessesByName("Steam"))
            {
                processExists = true;
                process.CloseMainWindow();
                while (!process.HasExited)
                    Thread.Sleep(500);
            }

            if (processExists)
                Thread.Sleep(1000);

            var newProcess = new Process();
            newProcess.StartInfo.FileName = vrstartup_exe_path;
            newProcess.Start();

            while (!Process.GetProcessesByName("vrmonitor").Any())
                Thread.Sleep(500);

            Debug.Log("SteamVR Restarted");
            IsRestarting = false;
        }
    }
}