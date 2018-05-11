using System.Linq;
using UnityEditor;
using UnityEngine;
using util = Litpla.VR.Util.SteamVR_DriverUtil;

namespace Litpla.VR.Util
{
    public class SteamVR_Preferences
    {
        private const string NO_REQUIRED_HMD = "NO_REQUIRED_HMD";

        private static bool IsNoRequiredHMD
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
                defines.Remove(define);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    string.Join(";", defines.ToArray()));
            }
        }

        [PreferenceItem("Litpla SteamVR")]
        private static void PreferencesGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            {
                if (util.IsRestarting)
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
                    var toggle = EditorGUILayout.Toggle(new GUIContent(title), IsNoRequiredHMD);
                    if (IsNoRequiredHMD && !toggle)
                    {
                        RemoveDefineSymbol(NO_REQUIRED_HMD);
                        util.DeactivateNoRequiredHMDSettings();
                    }
                    else if (!IsNoRequiredHMD && toggle)
                    {
                        AddDefineSymbol(NO_REQUIRED_HMD);
                        util.ActivateNoRequiredHMDSettings();
                    }
                    EditorGUILayout.HelpBox("HMD及びリンクボックスの接続を必要としない場合はTrueに", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}