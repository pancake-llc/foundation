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
        public static async void OnInspectorGUI()
        {
            Color previousColor;
            var ld = RegistryManager.IsInstalled("com.unity.services.leaderboards");
            if (ld.Item1) Uniform.DrawInstalled($"Leaderboard {ld.Item2}", new RectOffset(0, 0, 6, 0));

            var cloudSave = RegistryManager.IsInstalled("com.unity.services.cloudsave");
            if (cloudSave.Item1) Uniform.DrawInstalled($"CloudSave {cloudSave.Item2}", new RectOffset(0, 0, 6, 0));

            var playGame = RegistryManager.IsInstalled("com.google.play.games");
            if (playGame.Item1) Uniform.DrawInstalled($"Google Play Games 2.0.0", new RectOffset(0, 0, 6, 0));

            var appleSignIn = RegistryManager.IsInstalled("com.lupidan.apple-signin-unity");
            if (appleSignIn.Item1) Uniform.DrawInstalled($"Apple Signin 1.4.4", new RectOffset(0, 0, 6, 0));

            GUI.enabled = !EditorApplication.isCompiling;

            if (!ld.Item1)
            {
                if (GUILayout.Button("Install Package Leaderboard", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    await RegistryManager.InstallLastVersionForPacakge("com.unity.services.leaderboards");
                }
            }

            if (!cloudSave.Item1)
            {
                if (GUILayout.Button("Install Package CloudSave", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    await RegistryManager.InstallLastVersionForPacakge("com.unity.services.cloudsave");
                }
            }

            GUILayout.Space(4);
            var gameServiceSettings = Resources.Load<GameServiceSettings>(nameof(GameServiceSettings));
            if (gameServiceSettings == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                previousColor = GUI.backgroundColor;
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

                GUI.backgroundColor = previousColor;
                GUI.enabled = true;
            }
            else
            {
                var editorGameServiceSetting = UnityEditor.Editor.CreateEditor(gameServiceSettings);
                editorGameServiceSetting.OnInspectorGUI();
            }

            GUILayout.FlexibleSpace();
            if (ld.Item1)
            {
                previousColor = GUI.backgroundColor;
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

                GUI.backgroundColor = previousColor;
            }

            if (cloudSave.Item1)
            {
                previousColor = GUI.backgroundColor;
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

                GUI.backgroundColor = previousColor;
            }


            if (playGame.Item1)
            {
                previousColor = GUI.backgroundColor;
                GUI.backgroundColor = Uniform.Red_500;

                if (GUILayout.Button("Uninstall GPGS", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    bool confirmDelete = EditorUtility.DisplayDialog("Uninstall Google Play Game",
                        "" + "Are you sure you want to uninstall Google Play Game package ?\n" + "The GooglePlayGames folder will be deleted\n" +
                        "The GooglePlayGamesManifest.androidlib folder will be deleted",
                        "Yes",
                        "No");
                    if (confirmDelete)
                    {
                        FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "GooglePlayGamesManifest.androidlib"));
                        FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/Plugins/Android", "GooglePlayGamesManifest.androidlib.meta"));
                        FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "GooglePlayGames"));
                        FileUtil.DeleteFileOrDirectory(Path.Combine("Assets", "GooglePlayGames.meta"));
                        FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/GeneratedLocalRepo", "GooglePlayGames"));
                        FileUtil.DeleteFileOrDirectory(Path.Combine("Assets/GeneratedLocalRepo", "GooglePlayGames.meta"));
                        RegistryManager.RemovePackage("com.google.play.games");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        RegistryManager.Resolve();
                    }
                }

                GUI.backgroundColor = previousColor;
            }
            else
            {
                if (GUILayout.Button("Install Google Play Games", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    DebugEditor.Log("<color=#FF77C6>[Game Service]</color> importing google play game sdk v2.0.0");
                    RegistryManager.AddPackage("com.google.play.games",
                        "https://github.com/Thaina/play-games-plugin-for-unity.git?path=/Assets/Public/GooglePlayGames/com.google.play.games");
                    RegistryManager.Resolve();
                }
            }


            if (appleSignIn.Item1)
            {
                previousColor = GUI.backgroundColor;
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

                GUI.backgroundColor = previousColor;
            }
            else
            {
                if (GUILayout.Button("Install Apple SignIn", GUILayout.MaxHeight(Wizard.BUTTON_HEIGHT)))
                {
                    RegistryManager.AddPackage("com.lupidan.apple-signin-unity", "https://github.com/lupidan/apple-signin-unity.git#v1.4.4");
                    RegistryManager.Resolve();
                }
            }

            GUI.enabled = true;
        }
    }
}