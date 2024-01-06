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
        private SerializedProperty _enabledVisibilityProperty;
        private SerializedProperty _visibilityShowDuringPlayModeProperty;
        private SerializedProperty _enabledErrorProperty;
        private SerializedProperty _showIconOnParentProperty;
        private SerializedProperty _showScriptMissingProperty;
        private SerializedProperty _showReferenceNullProperty;
        private SerializedProperty _showReferenceIsMissingProperty;
        private SerializedProperty _showMissingEventMethodProperty;
        private SerializedProperty _showWhenTagOrLayerIsUndefinedProperty;
        private SerializedProperty _showForDisabledComponentsProperty;
        private SerializedProperty _showForDisabledGameObjectsProperty;
        private SerializedProperty _additionalIndentProperty;
        private SerializedProperty _additionalBackgroundColorProperty;
        private SerializedProperty _additionalActiveColorProperty;
        private SerializedProperty _additionalInactiveColorProperty;
        private SerializedProperty _additionalHideIconIfNotFitProperty;
        private SerializedProperty _additionalShowModifierWarningProperty;

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
                _enabledVisibilityProperty = serializedObject.FindProperty("enabledVisibility");
                _visibilityShowDuringPlayModeProperty = serializedObject.FindProperty("visibilityShowDuringPlayMode");
                _enabledErrorProperty = serializedObject.FindProperty("enabledError");
                _showIconOnParentProperty = serializedObject.FindProperty("showIconOnParent");
                _showScriptMissingProperty = serializedObject.FindProperty("showScriptMissing");
                _showReferenceNullProperty = serializedObject.FindProperty("showReferenceNull");
                _showReferenceIsMissingProperty = serializedObject.FindProperty("showReferenceIsMissing");
                _showMissingEventMethodProperty = serializedObject.FindProperty("showMissingEventMethod");
                _showWhenTagOrLayerIsUndefinedProperty = serializedObject.FindProperty("showWhenTagOrLayerIsUndefined");
                _showForDisabledComponentsProperty = serializedObject.FindProperty("showForDisabledComponents");
                _showForDisabledGameObjectsProperty = serializedObject.FindProperty("showForDisabledGameObjects");
                _additionalIndentProperty = serializedObject.FindProperty("additionalIndent");
                _additionalBackgroundColorProperty = serializedObject.FindProperty("additionalBackgroundColor");
                _additionalActiveColorProperty = serializedObject.FindProperty("additionalActiveColor");
                _additionalInactiveColorProperty = serializedObject.FindProperty("additionalInactiveColor");
                _additionalHideIconIfNotFitProperty = serializedObject.FindProperty("additionalHideIconIfNotFit");
                _additionalShowModifierWarningProperty = serializedObject.FindProperty("additionalShowModifierWarning");
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

            EditorGUILayout.PropertyField(_enabledVisibilityProperty, true);
            if (_enabledVisibilityProperty.boolValue)
            {
                EditorGUILayout.PropertyField(_visibilityShowDuringPlayModeProperty, new GUIContent("Show In Play Mode"), true);
            }

            EditorGUILayout.PropertyField(_enabledErrorProperty, true);
            if (_enabledErrorProperty.boolValue)
            {
                EditorGUILayout.PropertyField(_showIconOnParentProperty, true);
                EditorGUILayout.PropertyField(_showScriptMissingProperty, new GUIContent("Script Missing"), true);
                EditorGUILayout.PropertyField(_showReferenceNullProperty, new GUIContent("Null Reference"), true);
                EditorGUILayout.PropertyField(_showReferenceIsMissingProperty, new GUIContent("Missing Reference"), true);
                EditorGUILayout.PropertyField(_showMissingEventMethodProperty, new GUIContent("Missing Event Method"), true);
                EditorGUILayout.PropertyField(_showWhenTagOrLayerIsUndefinedProperty, new GUIContent("Tag Or Layer Undefined"), true);
                EditorGUILayout.PropertyField(_showForDisabledComponentsProperty, new GUIContent("Include Disabled Component"), true);
                EditorGUILayout.PropertyField(_showForDisabledGameObjectsProperty, new GUIContent("Include Disabled Object"), true);
            }

            EditorGUILayout.PropertyField(_additionalIndentProperty, new GUIContent("Indent"), true);
            EditorGUILayout.PropertyField(_additionalHideIconIfNotFitProperty, new GUIContent("Hide Icon If Not Fit"), true);
            EditorGUILayout.PropertyField(_additionalShowModifierWarningProperty, new GUIContent("Show Modifier Warning"), true);

            if (EditorGUIUtility.isProSkin)
            {
                EditorGUILayout.PropertyField(_additionalBackgroundColorProperty.FindPropertyRelative("dark"), new GUIContent("Background Color"), true);
                EditorGUILayout.PropertyField(_additionalActiveColorProperty.FindPropertyRelative("dark"), new GUIContent("Active Color"), true);
                EditorGUILayout.PropertyField(_additionalInactiveColorProperty.FindPropertyRelative("dark"), new GUIContent("Inactive Color"), true);
            }
            else
            {
                EditorGUILayout.PropertyField(_additionalBackgroundColorProperty.FindPropertyRelative("light"), new GUIContent("Background Color"), true);
                EditorGUILayout.PropertyField(_additionalActiveColorProperty.FindPropertyRelative("light"), new GUIContent("Active Color"), true);
                EditorGUILayout.PropertyField(_additionalInactiveColorProperty.FindPropertyRelative("light"), new GUIContent("Inactive Color"), true);
            }


            serializedObject.ApplyModifiedProperties();
        }
    }
}