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
            Debug = 0,
            CreateScriptableObject
        }

        private const string TITLE = "Wizard";
        private const int PAGE_COUNT = 2;
        private readonly string[] _headers = new[] {"Debug Extension Setup", "Create  Necessary ScriptableObject"};

        private readonly string[] _descriptions = new[]
        {
            "Enhance the Debug class with numerous improvements that can greatly improve readability of the Console view and save a lot of time by enabling more compact debugging code to be used.",
            "Create  necessary scriptableObject to hold setting"
        };

        private static readonly Vector2 WindowSize = new Vector2(400f, 280f);

        private static EWizardPage Page
        {
            get => (EWizardPage)EditorPrefs.GetInt($"wizard_{PlayerSettings.productGUID}_page", 0);
            set => EditorPrefs.GetInt($"wizard_{PlayerSettings.productGUID}_page", (int)value);
        }

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
            GUILayout.Label(_headers[(int) Page], Styles.title);
            GUILayout.Label(_descriptions[(int) Page], Styles.description);
            switch (Page)
            {
                case EWizardPage.Debug:
                    GUILayout.Label("Installation content", Styles.title);
                    GUILayout.Label("<color=#93DD59>1)</color> Debug dll for debug build and release build", Styles.description);
                    GUILayout.Label("<color=#93DD59>2)</color> ScriptableObject debug extension project setting in Resources folder", Styles.description);
                    GUILayout.Label("<color=#93DD59>3)</color> Script Chanel.cs", Styles.description);
                    break;
                case EWizardPage.CreateScriptableObject:
                    GUILayout.Label("Installation content", Styles.title);
                    GUILayout.Label("<color=#93DD59>1)</color> Database Setting", Styles.description);
                    break;
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Install", GUILayout.Height(28f), GUILayout.Width(100)))
            {
                string dir = "Assets/_Root/Resources/";
                switch (Page)
                {
                    case EWizardPage.Debug:
                        var debuglogExtensionProjectSettingInstance = CreateInstance<DebugLogExtensionsProjectSettingsAsset>();
                        if (!dir.DirectoryExists()) dir.CreateDirectory();
                        AssetDatabase.CreateAsset(debuglogExtensionProjectSettingInstance,
                            $"Assets/_Root/Resources/{DebugLogExtensionsProjectSettingsAsset.RESOURCE_PATH}.asset");
                        DebugLogExtensionsProjectSettingsAsset asset = DebugLogExtensionsProjectSettingsAsset.Get();
                        ChannelClassBuilder.BuildClass(asset.channels);

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
                    case EWizardPage.CreateScriptableObject:
                        var databaseAssetInstance = CreateInstance<Database.Database>();
                        if (!dir.DirectoryExists()) dir.CreateDirectory();
                        AssetDatabase.CreateAsset(databaseAssetInstance, $"Assets/_Root/Resources/{Database.Database.GLOBAL_DATABASE_NAME}.asset");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if ((int) Page + 1 < PAGE_COUNT)
                {
                    Page++;
                    EditorPrefs.SetInt($"wizard_{PlayerSettings.productGUID}_page", (int) Page);
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
            GUILayout.Label($"Page {(int) Page + 1}/{PAGE_COUNT}");
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