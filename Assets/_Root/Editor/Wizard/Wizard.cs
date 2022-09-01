using System;
using System.IO;
using Pancake.Debugging;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    internal class Wizard : EditorWindow
    {
        private enum EWizardPage
        {
            Debug = 0
        }

        private const string TITLE = "Wizard";
        private const int PAGE_COUNT = 1;
        private readonly string[] _headers = new[] {"Debug Extension Setup"};

        private readonly string[] _descriptions = new[]
        {
            "Enhance the Debug class with numerous improvements that can greatly improve readability of the Console view and save a lot of time by enabling more compact debugging code to be used.",
        };

        private static readonly Vector2 WindowSize = new Vector2(400f, 280f);
        private static EWizardPage page = EWizardPage.Debug;

        public static void Open()
        {
            var window = EditorWindow.GetWindow<Wizard>(true, TITLE, true);
            window.minSize = WindowSize;
            window.maxSize = WindowSize;
            window.ShowUtility();
        }

        private void OnGUI()
        {
            Styles.Init();
            GUILayout.Label(_headers[(int) page], Styles.title);
            GUILayout.Label(_descriptions[(int) page], Styles.description);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install", GUILayout.Height(28f), GUILayout.Width(100)))
            {
                switch (page)
                {
                    case EWizardPage.Debug:
                        var debuglogExtensionProjectSettingInstance = CreateInstance<DebugLogExtensionsProjectSettingsAsset>();
                        string dir = "Assets/_Root/Resources/";
                        if (!dir.DirectoryExists()) dir.CreateDirectory();
                        AssetDatabase.CreateAsset(debuglogExtensionProjectSettingInstance, $"Assets/_Root/Resources/{DebugLogExtensionsProjectSettingsAsset.RESOURCE_PATH}.asset");
                        var installerPath = AssetUtility.FindByNameAndExtension("debug_dll", ".unitypackage");
                        
                        if (!File.Exists(installerPath))
                        {
#if DEV_MODE
							Debug.LogWarning("Installer not found: "+installerPath);
#endif
                            return;
                        }
                        
#if DEV_MODE
						Debug.Log("Installing "+installerPath);
#endif

                        AssetDatabase.ImportPackage(installerPath, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if ((int) page + 1 < PAGE_COUNT)
                {
                    page++;
                }
                else
                {
                    EditorPrefs.SetBool($"wizard_{PlayerSettings.productGUID}", true);
                    EditorApplication.update -= AutoRunWizard.OnUpdate;
                    Close();
                }
            }

            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label($"Page {(int) page + 1}/{PAGE_COUNT}");
        }

        private static class Styles
        {
            private static bool initialized;
            public static GUIStyle title;
            public static GUIStyle description;

            public static void Init()
            {
                if (initialized) return;

                title = new GUIStyle(GUI.skin.label) {richText = true, fontSize = 18};
                SetTextColor(title, new Color(0.58f, 0.87f, 0.35f));

                description = new GUIStyle(GUI.skin.label) {fontSize = 12, wordWrap = true, richText = true};
                SetTextColor(description, new Color(0.93f, 0.93f, 0.93f));
            }

            private static void SetTextColor(GUIStyle style, Color color) =>
                style.normal.textColor = style.active.textColor = style.focused.textColor = style.hover.textColor =
                    style.onNormal.textColor = style.onActive.textColor = style.onFocused.textColor = style.onHover.textColor = color;
        }
    }
}