using System.IO;
using Pancake;
using Pancake.ExLibEditor;
using Pancake.Localization;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public static class UtilitiesLocalizationDrawer
    {
        public static void OnInspectorGUI(ref int index)
        {
#if PANCAKE_LOCALIZATION
            DrawTab(ref index);
            if (index == 0) DrawTabSetting();
            else DrawTabExplore();
#endif

#if !PANCAKE_LOCALIZATION
            GUI.enabled = !EditorApplication.isCompiling;
            if (GUILayout.Button("Enable Localization", GUILayout.MaxHeight(40f)))
            {
                ScriptingDefinition.AddDefineSymbolOnAllPlatforms("PANCAKE_LOCALIZATION");
                AssetDatabase.Refresh();
                RegistryManager.Resolve();
            }W
            GUI.enabled = true;
#endif
        }

        private static void DrawTab(ref int index)
        {
            EditorGUILayout.BeginHorizontal();

            DrawButtonSetting(ref index);
            DrawButtonExplore(ref index);
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawButtonSetting(ref int index)
        {
            bool clicked = GUILayout.Toggle(index == 0, "Settings", GUI.skin.button, GUILayout.ExpandWidth(true));
            index = clicked ? 0 : 1;
        }

        private static void DrawButtonExplore(ref int index)
        {
            bool clicked = GUILayout.Toggle(index == 1, "Explore", GUI.skin.button, GUILayout.ExpandWidth(true));
            index = clicked ? 1 : 0;
        }


        private static void DrawTabSetting()
        {
            var setting = Resources.Load<LocaleSettings>(nameof(LocaleSettings));
            if (setting == null)
            {
                GUI.enabled = !EditorApplication.isCompiling;
                GUI.backgroundColor = Uniform.Pink;
                if (GUILayout.Button("Create Localization Settings", GUILayout.Height(40f)))
                {
                    setting = ScriptableObject.CreateInstance<LocaleSettings>();
                    if (!Directory.Exists(Editor.DEFAULT_RESOURCE_PATH)) Directory.CreateDirectory(Editor.DEFAULT_RESOURCE_PATH);
                    AssetDatabase.CreateAsset(setting, $"{Editor.DEFAULT_RESOURCE_PATH}/{nameof(LocaleSettings)}.asset");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Debug.Log($"{nameof(LocaleSettings).TextColor("#f75369")} was created ad {Editor.DEFAULT_RESOURCE_PATH}/{nameof(LocaleSettings)}.asset");
                }

                GUI.backgroundColor = Color.white;
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.Space();
                var editor = UnityEditor.Editor.CreateEditor(setting);
                editor.OnInspectorGUI();
            }
        }

        private static void DrawTabExplore() { }
    }
}