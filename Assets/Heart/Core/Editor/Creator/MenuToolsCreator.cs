using System;
using JetBrains.Annotations;
using Pancake.Common;
using PancakeEditor.Finder;
using RedBlueGames.MulliganRenamer;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class MenuToolsCreator
    {
        #region 1

        [MenuItem("Tools/Pancake/Finder %#K")]
        private static void OpenFinder() { FinderWindow.ShowWindow(); }

        [MenuItem("Tools/Pancake/Finder - Delete User Settings")]
        private static void DeleteUserFinderSetting()
        {
            const string path = "UserSettings/FinderSetting.asset";
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                AssetDatabase.Refresh();
                Debug.Log("Success delete user settings of finder!");
            }
        }

        [MenuItem("Tools/Fast Play Mode", validate = false)]
        private static void ToggleFastPlayMode()
        {
            EditorSettings.enterPlayModeOptionsEnabled = !EditorSettings.enterPlayModeOptionsEnabled;
            AssetDatabase.Refresh();
            string text = EditorSettings.enterPlayModeOptionsEnabled ? " <color=#f75369>Enabled" : "<color=#FF2828>Disabled";
            Debug.Log($"Fast Play Mode {text}</color>");
        }

        [MenuItem("Tools/Fast Play Mode", validate = true)]
        private static bool ValidateToggleFastPlayMode()
        {
            Menu.SetChecked("Tools/Fast Play Mode", EditorSettings.enterPlayModeOptionsEnabled);
            return true;
        }

        [MenuItem("Tools/Require Scene Save", validate = false)]
        private static void ToggleRequireSceneSave()
        {
            Internal.InternalData.Settings.requireSceneSave = !Internal.InternalData.Settings.requireSceneSave;
            AssetDatabase.Refresh();
            string text = Internal.InternalData.Settings.requireSceneSave ? " <color=#f75369>Enabled" : "<color=#FF2828>Disabled";
            Debug.Log($"Require Scene Save {text}</color>");
        }

        [MenuItem("Tools/Require Scene Save", validate = true)]
        private static bool ValidateRequireSceneSave()
        {
            Menu.SetChecked("Tools/Require Scene Save", Internal.InternalData.Settings.requireSceneSave);
            return true;
        }

        [MenuItem("Tools/Pancake/Mulligan Renamer #R", false)]
        private static void MulliganRenamer() { EditorWindow.GetWindow<MulliganRenamerWindow>(false, "Mulligan Renamer", true); }

        [MenuItem("Tools/Pancake/BakingSheet/Runtime Csv Converter", validate = false)]
        private static void ToggleBakingSheetRuntimeCsv()
        {
            bool toggle = ScriptingDefinition.IsSymbolDefined("BAKINGSHEET_RUNTIME_CSVCONVERTER");
            toggle = !toggle;
            if (toggle)
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("BAKINGSHEET_RUNTIME_CSVCONVERTER");
            }
            else
            {
                ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("BAKINGSHEET_RUNTIME_CSVCONVERTER");
            }

            AssetDatabase.Refresh();
            string text = toggle ? " <color=#f75369>Enabled" : "<color=#FF2828>Disabled";
            Debug.Log($"[BakingSheet] Runtime Csv Converter {text}</color>");
        }

        [MenuItem("Tools/Pancake/BakingSheet/Runtime Csv Converter", validate = true)]
        private static bool ValidateToggleBakingSheetRuntimeCsv()
        {
            bool toggle = ScriptingDefinition.IsSymbolDefined("BAKINGSHEET_RUNTIME_CSVCONVERTER");
            Menu.SetChecked("Tools/Pancake/BakingSheet/Runtime Csv Converter", toggle);
            return true;
        }

        [MenuItem("Tools/Pancake/BakingSheet/Runtime Google Converter", validate = false)]
        private static void ToggleBakingSheetRuntimeGoogle()
        {
            bool toggle = ScriptingDefinition.IsSymbolDefined("BAKINGSHEET_RUNTIME_GOOGLECONVERTER");
            toggle = !toggle;
            if (toggle)
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("BAKINGSHEET_RUNTIME_GOOGLECONVERTER");
            }
            else
            {
                ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("BAKINGSHEET_RUNTIME_GOOGLECONVERTER");
            }

            AssetDatabase.Refresh();
            string text = toggle ? " <color=#f75369>Enabled" : "<color=#FF2828>Disabled";
            Debug.Log($"[BakingSheet] Runtime Google Converter {text}</color>");
        }

        [MenuItem("Tools/Pancake/BakingSheet/Runtime Google Converter", validate = true)]
        private static bool ValidateToggleBakingSheetRuntimeGoogle()
        {
            bool toggle = ScriptingDefinition.IsSymbolDefined("BAKINGSHEET_RUNTIME_GOOGLECONVERTER");
            Menu.SetChecked("Tools/Pancake/BakingSheet/Runtime Google Converter", toggle);
            return true;
        }

        #endregion

        #region 2

        [MenuItem("Tools/Project Settings", priority = 30000), UsedImplicitly]
        private static void OpenSettings()
        {
#if UNITY_2019_1_OR_NEWER
            SettingsService.OpenProjectSettings("Project/Player");
#else
			EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
#endif
        }

        [MenuItem("Tools/User Preferences", priority = 30000), UsedImplicitly]
        private static void OpenPreferences()
        {
#if UNITY_2019_1_OR_NEWER
            SettingsService.OpenUserPreferences("Preferences/External Tools");
#else
			EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
#endif
        }

        #endregion

        #region 3

        [MenuItem("Tools/Open Persistent Data Path", false, 31000), UsedImplicitly]
        private static void OpenPersistentDataPath() { EditorUtility.RevealInFinder(Application.persistentDataPath); }

        [MenuItem("Tools/Clear Persistent Data Path", false, 31000), UsedImplicitly]
        private static void ClearPersistentDataPath()
        {
            if (EditorUtility.DisplayDialog("Clear Persistent Data Path",
                    "Are you sure you wish to clear the persistent data path?\nThis action cannot be reversed.",
                    "Clear",
                    "Cancel"))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.persistentDataPath);

                foreach (var file in di.GetFiles()) file.Delete();
                foreach (var dir in di.GetDirectories()) dir.Delete(true);
                try
                {
                    Data.DeleteAll();
                }
                catch (Exception)
                {
                    //
                }
            }
        }

        [MenuItem("Tools/Clear PlayerPrefs", false, 31000), UsedImplicitly]
        private static void ClearPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog("Clear PlayerPrefs", "Are you sure you wish to clear PlayerPrefs?\nThis action cannot be reversed.", "Clear", "Cancel"))
                PlayerPrefs.DeleteAll();
        }

        #endregion
    }
}