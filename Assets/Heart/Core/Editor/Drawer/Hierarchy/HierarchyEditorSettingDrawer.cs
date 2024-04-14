using System;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    [CustomEditor(typeof(HierarchyEditorSetting), true)]
    public class HierarchyEditorSettingDrawer : UnityEditor.Editor
    {
        private SerializedProperty _enabledTreeMapProperty;
        private SerializedProperty _treeMapColorProperty;
        private SerializedProperty _treeMapEnhancedProperty;
        private SerializedProperty _treeMapTransparentBackgroundProperty;
        private SerializedProperty _enabledSeparatorProperty;
        private SerializedProperty _showRowShadingProperty;
        private SerializedProperty _separatorColorProperty;
        private SerializedProperty _evenRowColorProperty;
        private SerializedProperty _oddRowColorProperty;
        private SerializedProperty _additionalBackgroundColorProperty;

        private void OnEnable()
        {
            try
            {
                _enabledTreeMapProperty = serializedObject.FindProperty("enabledTreeMap");
                _treeMapColorProperty = serializedObject.FindProperty("treeMapColor");
                _treeMapEnhancedProperty = serializedObject.FindProperty("treeMapEnhanced");
                _treeMapTransparentBackgroundProperty = serializedObject.FindProperty("treeMapTransparentBackground");
                _enabledSeparatorProperty = serializedObject.FindProperty("enabledSeparator");
                _showRowShadingProperty = serializedObject.FindProperty("showRowShading");
                _separatorColorProperty = serializedObject.FindProperty("separatorColor");
                _evenRowColorProperty = serializedObject.FindProperty("evenRowColor");
                _oddRowColorProperty = serializedObject.FindProperty("oddRowColor");
                _additionalBackgroundColorProperty = serializedObject.FindProperty("additionalBackgroundColor");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_enabledTreeMapProperty, true);
            if (_enabledTreeMapProperty.boolValue)
            {
                if (EditorGUIUtility.isProSkin)
                {
                    EditorGUILayout.PropertyField(_treeMapColorProperty.FindPropertyRelative("dark"), new GUIContent("Tree Map Color"), true);
                }
                else
                {
                    EditorGUILayout.PropertyField(_treeMapColorProperty.FindPropertyRelative("light"), new GUIContent("Tree Map Color"), true);
                }

                EditorGUILayout.PropertyField(_treeMapEnhancedProperty, true);
                EditorGUILayout.PropertyField(_treeMapTransparentBackgroundProperty, new GUIContent("Transparent Background"), true);

                if (!_treeMapTransparentBackgroundProperty.boolValue)
                {
                    EditorGUILayout.PropertyField(
                        EditorGUIUtility.isProSkin
                            ? _additionalBackgroundColorProperty.FindPropertyRelative("dark")
                            : _additionalBackgroundColorProperty.FindPropertyRelative("light"),
                        new GUIContent("Background Color"),
                        true);
                }
            }

            EditorGUILayout.PropertyField(_enabledSeparatorProperty, true);
            if (_enabledSeparatorProperty.boolValue)
            {
                EditorGUILayout.PropertyField(_showRowShadingProperty, true);
                if (EditorGUIUtility.isProSkin)
                {
                    EditorGUILayout.PropertyField(_separatorColorProperty.FindPropertyRelative("dark"), new GUIContent("Seperator Color"), true);
                    EditorGUILayout.PropertyField(_evenRowColorProperty.FindPropertyRelative("dark"), new GUIContent("Even Row Color"), true);
                    EditorGUILayout.PropertyField(_oddRowColorProperty.FindPropertyRelative("dark"), new GUIContent("Odd Row Color"), true);
                }
                else
                {
                    EditorGUILayout.PropertyField(_separatorColorProperty.FindPropertyRelative("light"), new GUIContent("Seperator Color"), true);
                    EditorGUILayout.PropertyField(_evenRowColorProperty.FindPropertyRelative("light"), new GUIContent("Even Row Color"), true);
                    EditorGUILayout.PropertyField(_oddRowColorProperty.FindPropertyRelative("light"), new GUIContent("Odd Row Color"), true);
                }
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}