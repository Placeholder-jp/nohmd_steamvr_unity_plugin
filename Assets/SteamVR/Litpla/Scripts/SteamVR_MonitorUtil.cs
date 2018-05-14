using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Win32;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Litpla.VR.Util
{
    /// <summary>
    /// VRMonitor(Steamアプリケーション)の設定ファイルの書き換えや再起動を行う
    /// </summary>
    public static class SteamVR_MonitorUtil
    {
        /// <summary>
        /// SteamVRアプリケーションが再起動中
        /// </summary>
        public static bool IsRestarting { get; private set; }

        /// <summary>
        /// 一般設定をディスクから読み込み
        /// </summary>
        /// <returns></returns>
        private static steamvr_vrsettings Load_SteamVR_VRSettings()
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

        /// <summary>
        /// 仮想ドライバ設定をディスクから読み込み
        /// </summary>
        /// <returns></returns>
        private static default_vrsettings Load_Default_VRSettings()
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

        /// <summary>
        /// 一般設定をディスクへ書き込み
        /// </summary>
        /// <param name="data"></param>
        private static void WriteSteamVR_VRSettings(steamvr_vrsettings data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = Emviroment.Path.steamvr_vrsettings;
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// 仮想ドライバ設定をディスクへ書き込み
        /// </summary>
        /// <param name="data"></param>
        private static void WriteDefault_VRSettings(default_vrsettings data)
        {
            var json = JsonUtility.ToJson(data, true);
            var path = Emviroment.Path.default_vrsettings;
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// 既存のシャペロン設定を削除
        /// </summary>
        public static void DeleteChaperone_Info()
        {
            if (File.Exists(Emviroment.Path.chaperone_info_vrchap_path))
                File.Delete(Emviroment.Path.chaperone_info_vrchap_path);
        }

        /// <summary>
        /// 仮想HMDドライバを有効化します
        /// displayFrequencyはアプリ実行時のFixedDeltaTimeに影響します
        /// </summary>
        /// <param name="displayFrequency"></param>
        public static void EnableVirtualDisplayDriver(float displayFrequency = 60f)
        {
            var s = Load_SteamVR_VRSettings();
            s.steamvr.activateMultipleDrivers = true;
            s.steamvr.forcedDriver = "null";
            WriteSteamVR_VRSettings(s);

            var d = Load_Default_VRSettings();
            d.driver_null.enable = true;
            d.driver_null.displayFrequency = displayFrequency;
            WriteDefault_VRSettings(d);

            RestartSteamVR();
        }

        /// <summary>
        /// 仮想HMDドライバを無効化
        /// </summary>
        public static void DisableVirtualDisplayDriver()
        {
            var s = Load_SteamVR_VRSettings();
            s.steamvr.activateMultipleDrivers = false;
            s.steamvr.forcedDriver = string.Empty;
            WriteSteamVR_VRSettings(s);

            var d = Load_Default_VRSettings();
            d.driver_null.enable = false;
            d.driver_null.displayFrequency = 90f;
            WriteDefault_VRSettings(d);

            RestartSteamVR();
        }

        /// <summary>
        /// SteamVRアプリケーションを再起動
        /// 一般設定や仮想ドライバを有効にしたあと設定を有効にするには再起動が必要
        /// </summary>
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
            /// <summary>
            /// SteamVRアプリケーションに関連するプロセス名
            /// </summary>
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
                    get { return DefaultRoot; }
                }

                private static string DefaultRoot
                {
                    get
                    {
                        var path = string.Empty;
#if UNITY_STANDALONE_WIN
                        path = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Valve\\Steam", "SteamPath", "")
                            .ToString();
                        if (string.IsNullOrEmpty(path))
                            Debug.LogError("Could not find steam path in registry");
                        return path;
#else //TODO MacOS Support
                        return path;
#endif
                    }
                }

                /// <summary>
                /// 一般設定のファイルパス
                /// </summary>
                public static string steamvr_vrsettings
                {
                    get { return System.IO.Path.Combine(SteamApplicationPath, "config/steamvr.vrsettings"); }
                }

                /// <summary>
                /// シャペロン設定のファイルパス
                /// </summary>
                public static string chaperone_info_vrchap_path
                {
                    get { return System.IO.Path.Combine(SteamApplicationPath, "config/chaperone_info.vrchap"); }
                }

                /// <summary>
                /// 仮想ドライバ設定のファイルパス
                /// </summary>
                public static string default_vrsettings
                {
                    get
                    {
                        return System.IO.Path.Combine(SteamApplicationPath,
                            "steamapps/common/SteamVR/drivers/null/resources/settings/default.vrsettings");
                    }
                }

                /// <summary>
                /// SteamVRアプリケーション起動用exeのファイルパス
                /// </summary>
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