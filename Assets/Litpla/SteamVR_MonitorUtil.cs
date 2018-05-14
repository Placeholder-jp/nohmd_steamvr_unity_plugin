using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Litpla.VR.Util
{
    /// <summary>
    /// VRMonitor(Steamアプリケーション)の設定ファイルの書き換えや再起動を行う
    /// </summary>
    public static class SteamVR_MonitorUtil
    {
        public static bool IsRestarting { get; private set; }

        private static steamvr_vrsettings ReadSteamVR_VRSettings()
        {
            var data = new steamvr_vrsettings();
            var path = Emviroment.Path.steamvr_vrsettings;
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
            var path = Emviroment.Path.default_vrsettings;
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
            Debug.Log(json);
            var path = Emviroment.Path.steamvr_vrsettings;
            File.WriteAllText(path, json);
        }

        private static void WriteDefault_VRSettings(default_vrsettings data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = Emviroment.Path.default_vrsettings;
            File.WriteAllText(path, json);
        }

        public static void DeleteChaperone_Info()
        {
            if (File.Exists(Emviroment.Path.chaperone_info_vrchap_path))
            {
                File.Delete(Emviroment.Path.chaperone_info_vrchap_path);
            }
        }

        /// <summary>
        /// 仮想HMDドライバを有効化します
        /// displayFrequencyはアプリ実行時のFixedDeltaTimeに影響します
        /// </summary>
        /// <param name="displayFrequency"></param>
        public static void EnableVirtualDisplayDriver(float displayFrequency = 60f)
        {
            var s = ReadSteamVR_VRSettings();
            s.steamvr.activateMultipleDrivers = true;
            s.steamvr.forcedDriver = "null";
            WriteSteamVR_VRSettings(s);

            var d = ReadDefault_VRSettings();
            d.driver_null.enable = true;
            d.driver_null.displayFrequency = displayFrequency;
            WriteDefault_VRSettings(d);

            RestartSteamVR();
        }

        public static void DisableVirtualDisplayDriver()
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
            var processExists = false;

            foreach (var processName in Emviroment.ProcessNames)
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    processExists = true;
                    process.CloseMainWindow();
                    while (!process.HasExited)
                        Thread.Sleep(500);
                }

            if (processExists)
                Thread.Sleep(1000);

            var newProcess = new Process();
            newProcess.StartInfo.FileName = Emviroment.Path.vrstartup_exe;
            newProcess.Start();

            while (!Process.GetProcessesByName(Emviroment.ProcessNames[0]).Any())
                Thread.Sleep(500);

            IsRestarting = false;
        }

        private static class Emviroment
        {
            public static readonly string[] ProcessNames =
            {
                "vrmonitor",
                "vrcompositor",
                "vrserver"
            };

            public static class Path
            {
                private static string SteamApplicationPath
                {
                    get { return string.IsNullOrEmpty(CustomRoot) ? DefaultRoot : CustomRoot; }
                }

                private static string DefaultRoot
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
                        path = System.IO.Path.Combine(path, "Steam");
                        return path;
                    }
                }

                //TODO 外部ファイルから設定できるように
                private static string CustomRoot
                {
                    get { return string.Empty; }
                }

                public static string steamvr_vrsettings
                {
                    get { return System.IO.Path.Combine(SteamApplicationPath, "config/steamvr.vrsettings"); }
                }

                public static string chaperone_info_vrchap_path
                {
                    get { return System.IO.Path.Combine(SteamApplicationPath, "config/chaperone_info.vrchap"); }
                }

                public static string default_vrsettings
                {
                    get
                    {
                        return System.IO.Path.Combine(SteamApplicationPath,
                            "steamapps/common/SteamVR/drivers/null/resources/settings/default.vrsettings");
                    }
                }

                public static string vrstartup_exe
                {
                    get
                    {
                        return System.IO.Path.Combine(SteamApplicationPath,
                            "steamapps/common/SteamVR/bin/win64/vrstartup.exe");
                    }
                }
            }
        }
    }
}