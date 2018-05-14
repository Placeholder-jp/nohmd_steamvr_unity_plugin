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

            if (Monitor.IsRestarting)
            {
                EditorGUILayout.HelpBox("Restarting SteamVR...", MessageType.Info);
                return;
            }

            if (EditorApplication.isCompiling)
            {
                EditorGUILayout.HelpBox("Recompiling...", MessageType.Info);
                return;
            }

            #region HMD Requirement
            var title = "No Require HMD connection";
            var toggle = EditorGUILayout.Toggle(new GUIContent(title), HasNoRequiredHMDDefineSymbol);
            if (HasNoRequiredHMDDefineSymbol && !toggle)
                DeactivateNoRequiredHMDSettings();
            else if (!HasNoRequiredHMDDefineSymbol && toggle)
                ActivateNoRequiredHMDSettings();
            EditorGUILayout.HelpBox("HMD及びリンクボックスの接続を必要としない場合はTrueに", MessageType.Info);
            #endregion

            #region Virtual display frequency
            EditorGUILayout.Space();
            if (HasNoRequiredHMDDefineSymbol)
            {
                title = "Virtural HMD frequency";
                var current = EditorPrefs.GetFloat("SteamVR_Virtual_HMD_Rate", 60f);
                var field = EditorGUILayout.FloatField(new GUIContent(title), current);
                if (current != field)
                {
                    EditorPrefs.SetFloat("SteamVR_Virtual_HMD_Rate", field);
                }
                EditorGUILayout.HelpBox("仮想HMDドライバのディスプレイのリフレッシュレート\n※アプリ実行時のFixedDeltaTimeに影響します", MessageType.Info);
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