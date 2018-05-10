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
                if (GUILayout.Button("ReadSteamVRSettings"))
                {
                    var newSettings = new steamvr_vrsettings();
                    var json = JsonUtility.ToJson(newSettings);
                    Debug.Log(json);
                }

                if (GUILayout.Button("ReadDriver"))
                {
                    var newDriver = new default_vrsettings();
                    var json = JsonUtility.ToJson(newDriver);
                    Debug.Log(json);
                }

                if (EditorApplication.isCompiling)
                {
                    EditorGUILayout.HelpBox("Recompiling...", MessageType.Info);
                }
                else
                {
                    var title = "No Require HMD connection";
                    var toggle = EditorGUILayout.Toggle(new GUIContent(title), IsNoRequiredHMD);
                    if (IsNoRequiredHMD && !toggle)
                        RemoveDefineSymbol(NO_REQUIRED_HMD);
                    else if (!IsNoRequiredHMD && toggle)
                        AddDefineSymbol(NO_REQUIRED_HMD);
                    EditorGUILayout.HelpBox("HMD及びリンクボックスの接続を必要としない場合はTrueに", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}