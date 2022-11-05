#if PANCAKE_PLAYFAB
using Pancake.GameService;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class ServiceSettingWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private UnityEditor.Editor _editor;

        private void OnGUI()
        {
            if (_editor == null) _editor = UnityEditor.Editor.CreateEditor(ServiceSettings.Instance);

            if (_editor == null)
            {
                EditorGUILayout.HelpBox("Coundn't create the settings resources editor.", MessageType.Error);
                return;
            }

            ServiceSettingEditor.callFromEditorWindow = true;
            _editor.DrawHeader();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.BeginVertical(new GUIStyle {padding = new RectOffset(6, 3, 3, 3)});
            _editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            ServiceSettingEditor.callFromEditorWindow = false;
        }

        private static ServiceSettingWindow GetWindow()
        {
            var window = GetWindow<ServiceSettingWindow>(InEditor.InspectorWindow);
            window.titleContent = new GUIContent("Playfab Setting");

            return window;
        }

        public static void ShowWindow()
        {
            var window = GetWindow();
            if (window == null)
            {
                Debug.LogError("Coundn't open the playfab settings window.");
                return;
            }

            window.minSize = new Vector2(280, 0);
            window.Show();
        }

        private void OnEnable() { Uniform.FoldoutSettings.LoadSetting(); }

        private void OnDisable()
        {
            Uniform.FoldoutSettings.SaveSetting();
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif