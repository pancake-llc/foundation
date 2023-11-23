using System;
using System.Collections.Generic;
using System.Linq;
using Pancake.Localization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Pancake.LocalizationEditor
{
    [CustomEditor(typeof(LocaleSettings), true)]
    public class LocaleSettingsEditor : UnityEditor.Editor
    {
        private ReorderableList _reorderableList;
        private SerializedProperty _avaiableLanguageProperty;
        private SerializedProperty _importLocationProperty;
        private SerializedProperty _googleCredentialProperty;

        private void Init()
        {
            _avaiableLanguageProperty = serializedObject.FindProperty("availableLanguages");
            _importLocationProperty = serializedObject.FindProperty("importLocation");
            _googleCredentialProperty = serializedObject.FindProperty("googleCredential");

            if (_avaiableLanguageProperty != null)
            {
                _reorderableList = new ReorderableList(serializedObject,
                    _avaiableLanguageProperty,
                    true,
                    true,
                    true,
                    true);

                _reorderableList.drawHeaderCallback = OnDrawHeaderCallback;
                _reorderableList.drawElementCallback = OnDrawElementCallback;
                _reorderableList.onAddDropdownCallback += OnAddCallback;
                _reorderableList.onRemoveCallback += OnRemoveCallback;
                _reorderableList.onCanRemoveCallback += OnCanRemoveCallback;
            }
        }

        private bool OnCanRemoveCallback(ReorderableList list)
        {
            return false;
        }

        private void OnDrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            var languageProperty = _reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var position = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);

            var isCustom = languageProperty.FindPropertyRelative("custom").boolValue;
            if (isCustom)
            {
                var languageName = languageProperty.FindPropertyRelative("name");
                var languageCode = languageProperty.FindPropertyRelative("code");

                var labelWidth = EditorGUIUtility.labelWidth;

                EditorGUIUtility.labelWidth = 40;
                var r1 = new Rect(position.x, position.y, position.width / 2 - 2, position.height);
                EditorGUI.PropertyField(r1, languageName, new GUIContent(languageName.displayName, "Language name"));

                EditorGUIUtility.labelWidth = 40;
                var r2 = new Rect(position.x + r1.width + 4, position.y, position.width / 2 - 2, position.height);
                EditorGUI.PropertyField(r2, languageCode, new GUIContent(languageCode.displayName, "ISO-639-1 code"));

                EditorGUIUtility.labelWidth = labelWidth;
            }
            else
            {
                Helper.LanguageField(position, languageProperty, GUIContent.none, true);
            }
        }

        
        private void OnDrawHeaderCallback(Rect rect) { EditorGUI.LabelField(rect, "Available Languages"); }


        private void OnAddCallback(Rect buttonrect, ReorderableList list) { }

        private void OnRemoveCallback(ReorderableList list) { }


        public override void OnInspectorGUI() { base.OnInspectorGUI(); }
    }
}