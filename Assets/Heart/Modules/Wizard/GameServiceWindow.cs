using System.IO;
using Pancake;
using Pancake.Common;
using Pancake.SignIn;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    internal static class GameServiceWindow
    {
        public static void OnInspectorGUI()
        {
#if PANCAKE_LEADERBOARD
            Uniform.DrawInstalled("Leaderboard 2.2.0");
#endif
#if PANCAKE_CLOUDSAVE
            Uniform.DrawInstalled("CloudSave 3.2.1");
#endif

#if PANCAKE_APPLE_SIGNIN
            Uniform.DrawInstalled("Apple SignIn 1.4.3");
#endif

            GUI.enabled = !EditorApplication.isCompiling;
#if !PANCAKE_LEADERBOARD
            if (GUILayout.Button("Install Package Leaderboard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.unity.services.leaderboards", "2.2.0");
                RegistryManager.Resolve();
            }
#endif

#if !PANCAKE_CLOUDSAVE
            if (GUILayout.Button("Install Package CloudSave", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.unity.services.cloudsave", "3.2.1");
                RegistryManager.Resolve();
            }
#endif

            GUILayout.Space(4);
            var gameServiceSettings = Resources.Load<GameServiceSettings>(nameof(GameServiceSettings));
            if (gameServiceSettings == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink_500;
                if (GUILayout.Button("Create Game Service Setting", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    var setting = ScriptableObject.CreateInstance<GameServiceSettings>();
                    if (!Directory.Exists(Common.Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Common.Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(GameServiceSettings)}.asset");
                    Debug.Log(
                        $"{nameof(GameServiceSettings).SetColor("f75369")} was created ad {Common.Editor.DEFAULT_RESOURCE_PATH}/{nameof(GameServiceSettings)}.asset");

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                var editorGameServiceSetting = UnityEditor.Editor.CreateEditor(gameServiceSettings);
                editorGameServiceSetting.OnInspectorGUI();
            }

            GUILayout.FlexibleSpace();
#if PANCAKE_LEADERBOARD
            GUI.backgroundColor = Uniform.Red_500;
            if (GUILayout.Button("Uninstall Leaderboard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Leaderboard", "Are you sure you want to uninstall leaderboard package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.unity.services.leaderboards");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#endif

#if PANCAKE_CLOUDSAVE
            GUI.backgroundColor = Uniform.Red_500;
            if (GUILayout.Button("Uninstall CloudSave", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall CloudSave", "Are you sure you want to uninstall cloud save package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.unity.services.cloudsave");
                    RegistryManager.Resolve();
                }
            }

            GUI.backgroundColor = Color.white;
#if !PANCAKE_GPGS
            EditorGUILayout.BeginHorizontal();

            bool gpgsInstalled = File.Exists("Assets/GooglePlayGames/com.google.play.games/Runtime/Google.Play.Games.asmdef");

            var contentInstallLabel = "Install GPGS v2.0.0 (1)";
            if (gpgsInstalled)
            {
                GUI.backgroundColor = Uniform.Green_500;
                contentInstallLabel = "GPGS v2.0.0 Installed (1)";
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }

            if (GUILayout.Button(contentInstallLabel, GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                DebugEditor.Log("<color=#FF77C6>[Game Service]</color> importing google play game sdk v2.0.0");
                AssetDatabase.ImportPackage(ProjectDatabase.GetPathInCurrentEnvironent("Editor/UnityPackages/gpgs.unitypackage"), false);
            }

            var previousColor = GUI.color;
            if (gpgsInstalled) GUI.color = Uniform.Green_500;

            GUILayout.Label(" =====> ", GUILayout.Width(52), GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT));
            GUI.color = previousColor;
            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Add GPGS Symbol (2)", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                if (!ScriptingDefinition.IsSymbolDefined("PANCAKE_GPGS"))
                {
                    ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_GPGS");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            EditorGUILayout.EndHorizontal();
#else
            GUI.backgroundColor = Uniform.Red_500;

            if (GUILayout.Button("Uninstall GPGS v2.0.0", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
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
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            GUI.backgroundColor = Color.white;
#endif


#if !PANCAKE_APPLE_SIGNIN
            if (GUILayout.Button("Install Apple SignIn 1.4.3", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                RegistryManager.AddPackage("com.lupidan.apple-signin-unity", "https://github.com/lupidan/apple-signin-unity.git#v1.4.3");
                RegistryManager.Resolve();
            }
#else
            GUI.backgroundColor = Uniform.Red_500;
            if (GUILayout.Button("Uninstall Apple SignIn", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
            {
                bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Apple SignIn", "Are you sure you want to uninstall apple signin package ?", "Yes", "No");
                if (confirmDelete)
                {
                    RegistryManager.RemovePackage("com.lupidan.apple-signin-unity");
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