using Pancake.SaveData;
using UnityEngine;
using UnityEditor;

namespace Pancake.Editor.SaveData
{
    internal class SettingsWindow : SubWindow
    {
        public DefaultSetting editorSettings;
        public Setting settings;
        public SerializedObject so = null;

        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public SerializedProperty assemblyNamesProperty = null;

        private Vector2 _scrollPos = Vector2.zero;

        public SettingsWindow(EditorWindow window)
            : base("Settings", window)
        {
        }

        public override void OnGUI()
        {
            if (settings == null || editorSettings == null || assemblyNamesProperty == null) Init();

            var style = EditorStyle.Get;

            var labelWidth = EditorGUIUtility.labelWidth;


            EditorGUI.BeginChangeCheck();

            using (var scrollView = new EditorGUILayout.ScrollViewScope(_scrollPos, style.area))
            {
                _scrollPos = scrollView.scrollPosition;

                EditorGUIUtility.labelWidth = 160;

                GUILayout.Label("Runtime Settings", style.heading);

                using (new EditorGUILayout.VerticalScope(style.area))
                {
                    SettingsEditor.Draw(settings);
                }

                GUILayout.Label("Debug Settings", style.heading);

                using (new EditorGUILayout.VerticalScope(style.area))
                {
                    EditorGUIUtility.labelWidth = 100;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel("Log Info");
                        editorSettings.logInfo = EditorGUILayout.Toggle(editorSettings.logInfo);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel("Log Warnings");
                        editorSettings.logWarnings = EditorGUILayout.Toggle(editorSettings.logWarnings);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel("Log Errors");
                        editorSettings.logErrors = EditorGUILayout.Toggle(editorSettings.logErrors);
                    }

                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginHorizontal(style.area);

                if (GUILayout.Button("Open Persistent Data Path")) OpenPersistentDataPath();

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal(style.area);

                if (GUILayout.Button("Clear Persistent Data Path")) ClearPersistentDataPath();
                if (GUILayout.Button("Clear PlayerPrefs")) ClearPlayerPrefs();

                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(editorSettings);

            EditorGUIUtility.labelWidth = labelWidth; // Set the label width back to default
        }

        public void Init()
        {
            editorSettings = MetaData.DefaultSetting;

            settings = editorSettings.settings;
            /*so = new SerializedObject(editorSettings);
            var settingsProperty = so.FindProperty("settings");
            assemblyNamesProperty = settingsProperty.FindPropertyRelative("assemblyNames");*/
        }

        [MenuItem("Tools/Pancake/Archive/Open Persistent Data Path", false, 200)]
        private static void OpenPersistentDataPath() { EditorUtility.RevealInFinder(Application.persistentDataPath); }

        [MenuItem("Tools/Pancake/Archive/Clear Persistent Data Path", false, 200)]
        private static void ClearPersistentDataPath()
        {
            if (EditorUtility.DisplayDialog("Clear Persistent Data Path",
                    "Are you sure you wish to clear the persistent data path?\n This action cannot be reversed.",
                    "Clear",
                    "Cancel"))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(Application.persistentDataPath);

                foreach (System.IO.FileInfo file in di.GetFiles())
                    file.Delete();
                foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
                    dir.Delete(true);
            }
        }

        [MenuItem("Tools/Pancake/Archive/Clear PlayerPrefs", false, 200)]
        private static void ClearPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog("Clear PlayerPrefs", "Are you sure you wish to clear PlayerPrefs?\nThis action cannot be reversed.", "Clear", "Cancel"))
                PlayerPrefs.DeleteAll();
        }
    }
}