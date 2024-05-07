using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

namespace Pancake.ReplacerEditor
{
    public class ReplaceFontEditorWindow : EditorWindow
    {
        private static float sizeX = 350;
        private static float sizeY = 95;
        private static EditorWindow window;

        private Font _newFont;
        private TMP_FontAsset _newTMPfont;
        [SerializeField] private List<UnityEngine.UI.Text> specifiedLegacyTextObjects = new();
        [SerializeField] private List<TextMeshProUGUI> specifiedTMPObjects = new();
        private WindowType _windowType;
        private FontType _fontType;
        private Vector2 _scrollPosition = Vector2.zero;

        public static void ShowWindow()
        {
            window = GetWindow(typeof(ReplaceFontEditorWindow));
            window.titleContent = new GUIContent("Replace font Tool");
            window.maxSize = window.minSize = new Vector2(sizeX, sizeY);
        }

        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,
                false,
                Mathf.Approximately(sizeY, 500f),
                GUILayout.Width(window.maxSize.x),
                GUILayout.Height(window.maxSize.y));

            GUILayout.Space(10);

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty specifiedLegacyTextObjectsProperty = so.FindProperty(nameof(specifiedLegacyTextObjects));
            SerializedProperty specifiedTMPObjectsProperty = so.FindProperty(nameof(specifiedTMPObjects));

            if (_fontType == FontType.LegacyText) _newFont = EditorGUILayout.ObjectField("New font", _newFont, typeof(Font), false) as Font;
            else _newTMPfont = EditorGUILayout.ObjectField("New font", _newTMPfont, typeof(TMP_FontAsset), false) as TMP_FontAsset;

            if (_windowType == WindowType.Specified)
            {
                EditorGUILayout.PropertyField(_fontType == FontType.LegacyText ? specifiedLegacyTextObjectsProperty : specifiedTMPObjectsProperty, true);
                sizeY = 95 + EditorGUI.GetPropertyHeight(_fontType == FontType.LegacyText ? specifiedLegacyTextObjectsProperty : specifiedTMPObjectsProperty, true) +
                        (_fontType == FontType.LegacyText ? (specifiedLegacyTextObjects.Count != 0 ? 20 : 0) : (specifiedTMPObjects.Count != 0 ? 20 : 0));
                sizeY = Mathf.Clamp(sizeY, 90, 500);
            }
            else sizeY = 95;

            _windowType = (WindowType) EditorGUILayout.EnumPopup("Find place:", _windowType);

            _fontType = (FontType) EditorGUILayout.EnumPopup("Type:", _fontType);

            if (GUILayout.Button("Replace"))
            {
                switch (_windowType)
                {
                    case WindowType.CurrentScene:
                        bool c = EditorUtility.DisplayDialog("Replace All Font In Scene",
                            "Are you sure you want to replace all font in current scene?" + "\nText is a part of prefab in scene will be ignore",
                            "Yes",
                            "No");
                        if (c)
                        {
                            if (_fontType == FontType.LegacyText) ReplaceFont.ReplaceFontInScene(_newFont);
                            else ReplaceFont.ReplaceFontInScene(_newTMPfont);
                        }

                        break;

                    case WindowType.Prefabs:
                        if (_fontType == FontType.LegacyText) ReplaceFont.ReplaceFontPrefab(_newFont);
                        else ReplaceFont.ReplaceFontPrefab(_newTMPfont);
                        break;

                    case WindowType.Specified:
                        if (_fontType == FontType.LegacyText) ReplaceFont.ReplaceFontSpecified(_newFont, specifiedLegacyTextObjects);
                        else ReplaceFont.ReplaceFontSpecified(_newTMPfont, specifiedTMPObjects);
                        break;
                }
            }

            if (_windowType == WindowType.Specified)
            {
                bool listIsNotEmpty = _fontType == FontType.LegacyText ? specifiedLegacyTextObjects.Count != 0 : specifiedTMPObjects.Count != 0;
                if (listIsNotEmpty)
                    if (GUILayout.Button("Clear"))
                    {
                        if (_fontType == FontType.LegacyText) specifiedLegacyTextObjects.Clear();
                        else specifiedTMPObjects.Clear();
                    }
            }

            GUILayout.EndScrollView();
            so.ApplyModifiedProperties();

            if (window != null)
            {
                if (Mathf.Approximately(sizeY, 500f)) sizeX = 350 + 13;
                else sizeX = 350;

                window.maxSize = window.minSize = new Vector2(sizeX, sizeY);
            }
        }
    }

    public enum WindowType
    {
        [InspectorName("Current scene")] CurrentScene,
        [InspectorName("Al Prefab")] Prefabs,
        Specified
    }

    public enum FontType
    {
        LegacyText,
        TextMeshPro
    }
}