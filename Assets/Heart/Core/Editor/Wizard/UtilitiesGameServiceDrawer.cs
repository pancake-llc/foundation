using System.IO;
using Pancake;
using Pancake.ExLib;
using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesGameServiceDrawer
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_LEADERBOARD
            Uniform.DrawInstalled("Leaderboard Version 2.0.0");
#endif
#if PANCAKE_CLOUDSAVE
            Uniform.DrawInstalled("CloudSave Version 3.1.0");
#endif

#if PANCAKE_APPLE_SIGNIN
            Uniform.DrawInstalled("Apple SignIn Version 1.4.3");
#endif

            GUI.enabled = !EditorApplication.isCompiling;
#if !PANCAKE_LEADERBOARD
            if (GUILayout.Button("Install Package Leaderboard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.unity.services.leaderboards", "2.0.0");
                RegistryManager.Resolve();
            }
#endif

#if !PANCAKE_CLOUDSAVE
            if (GUILayout.Button("Install Package CloudSave", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.unity.services.cloudsave", "3.1.0");
                RegistryManager.Resolve();
            }
#endif

            GUILayout.FlexibleSpace();
#if PANCAKE_LEADERBOARD
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Leaderboard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Leaderboard", "Are you sure you want to uninstall leaderboard package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.services.leaderboards");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#endif

#if PANCAKE_CLOUDSAVE
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall CloudSave", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall CloudSave", "Are you sure you want to uninstall cloud save package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.unity.services.cloudsave");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#if !PANCAKE_GPGS
            EditorGUILayout.BeginHorizontal();

            bool gpgsInstalled = File.Exists("Assets/GooglePlayGames/com.google.play.games/Runtime/Google.Play.Games.asmdef");

            string contentInstallLabel = "Install GPGS v11.01 (1)";
            if (gpgsInstalled)
            {
                GUI.backgroundColor = Uniform.Green;
                contentInstallLabel = "GPGS v11.01 Installed (1)";
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button(contentInstallLabel, GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                DebugEditor.Log("<color=#FF77C6>[Game Service]</color> importing google play game sdk 11.01");
                AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Modules/Apex/ExLib/Core/Editor/Misc/UnityPackages/gpgs.unitypackage"), false);
            }

            var previousColor = GUI.color;
            if (gpgsInstalled) GUI.color = Uniform.Green;

            GUILayout.Label(" =====> ", new GUIStyle(EditorStyles.label) {padding = new RectOffset(0, 0, 5, 0)}, GUILayout.Width(52));
            GUI.color = previousColor;
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Add GPGS Symbol (2)", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                var group = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
                if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_GPGS", group))
                {
                    ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_GPGS");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            
            EditorGUILayout.EndHorizontal();
#else
            GUI.backgroundColor = Uniform.Red;

            if (GUILayout.Button("Uninstall GPGS v11.01", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Google Play Game",
                    "" + "Are you sure you want to uninstall Google Play Game package ?\n" + "The GooglePlayGames folder will be deleted\n" +
                    "The GooglePlayGamesManifest.androidlib folder will be deleted",
                    "Yes",
                    "No");
                if (confirmDelete)
                {
                    ScriptingDefinition.RemoveDefineSymbolOnAllPlatforms("PANCAKE_GPGS");
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "GooglePlayGamesManifest.androidlib"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "GooglePlayGamesManifest.androidlib.meta"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "GooglePlayGames"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "GooglePlayGames.meta"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/GeneratedLocalRepo", "GooglePlayGames"));
                    FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/GeneratedLocalRepo", "GooglePlayGames.meta"));
                    AssetDatabase.Refresh();
                }
            }

            GUI.backgroundColor = Color.white;
#endif


#if !PANCAKE_APPLE_SIGNIN
            if (GUILayout.Button("Install Apple SignIn 1.4.3", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.Add("com.lupidan.apple-signin-unity", "https://github.com/lupidan/apple-signin-unity.git#v1.4.3");
                RegistryManager.Resolve();
            }
#else
            GUI.backgroundColor = Uniform.Red;
            if (GUILayout.Button("Uninstall Apple SignIn", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Apple SignIn", "Are you sure you want to uninstall apple signin package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.Remove("com.lupidan.apple-signin-unity");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#endif
#endif
            GUI.enabled = true;
        }
    }
}