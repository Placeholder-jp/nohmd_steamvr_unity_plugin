using System.Linq;
using UnityEditor;
using UnityEngine;
using Monitor = Litpla.VR.Util.SteamVR_MonitorUtil;

namespace Litpla.VR.Util
{
    /// <summary>
    /// SteamVRプラグインでは変更できない設定を行うためのクラス
    /// </summary>
    public class SteamVRPrefs
    {
        private const string NO_REQUIRED_HMD = "NO_REQUIRED_HMD";

        [PreferenceItem("SteamVR Util")]
        private static void PreferencesGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            #region HMD Requirement
            if (Monitor.IsRestarting)
            {
                EditorGUILayout.HelpBox("Restarting SteamVR...", MessageType.Info);
            }
            else if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("Recompiling...", MessageType.Info);
            }
            else
            {
                var title = "No Require HMD connection";
                var toggle = EditorGUILayout.Toggle(new GUIContent(title), HasNoRequiredHMDDefineSymbol);
                if (HasNoRequiredHMDDefineSymbol && !toggle)
                    DeactivateNoRequiredHMDSettings();
                else if (!HasNoRequiredHMDDefineSymbol && toggle)
                    ActivateNoRequiredHMDSettings();
                EditorGUILayout.HelpBox("HMD及びリンクボックスの接続を必要としない場合はTrueに", MessageType.Info);
            }
            #endregion
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// HMD接続を不要にする設定を有効化
        /// </summary>
        private static void ActivateNoRequiredHMDSettings()
        {
            AddDefineSymbol(NO_REQUIRED_HMD);
            Monitor.EnableVirtualDisplayDriver();
        }

        /// <summary>
        /// HMD接続を不要にする設定を無効化
        /// </summary>
        private static void DeactivateNoRequiredHMDSettings()
        {
            RemoveDefineSymbol(NO_REQUIRED_HMD);
            Monitor.DisableVirtualDisplayDriver();
        }

        private static bool HasNoRequiredHMDDefineSymbol
        {
            get
            {
                var defines =
                    PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                return defines.Contains(NO_REQUIRED_HMD);
            }
        }

        private static void AddDefineSymbol(string define)
        {
            var defines = PlayerSettings
                .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';')
                .ToList();
            defines.Add(define);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", defines.ToArray()));
        }

        private static void RemoveDefineSymbol(string define)
        {
            var defines = PlayerSettings
                .GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';')
                .ToList();
            if (defines.Contains(define))
            {
                defines.RemoveAll(s => s == NO_REQUIRED_HMD);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", defines.ToArray()));
            }
        }
    }
}