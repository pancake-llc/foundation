using Pancake;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [CustomEditor(typeof(HeartSettings), true)]
    public class HeartSettingDrawer : UnityEditor.Editor
    {
        private SerializedProperty _debugViewProperty;
        private SerializedProperty _enablePrivacyFirstOpenProperty;
        private SerializedProperty _enableMultipleTouchProperty;
        private SerializedProperty _requireInternetProperty;
        private SerializedProperty _targetFrameRateProperty;
        private SerializedProperty _termOfServiceUrlProperty;
        private SerializedProperty _privacyUrlProperty;
        private SerializedProperty _privacyTitleProperty;
        private SerializedProperty _privacyMessageProperty;
#if UNITY_IOS
        private SerializedProperty _appstoreAppIdProperty;
        private SerializedProperty _skAdConversionValueProperty;
#endif

        private void OnEnable()
        {
            _debugViewProperty = serializedObject.FindProperty("debugView");
            _enablePrivacyFirstOpenProperty = serializedObject.FindProperty("enablePrivacyFirstOpen");
            _enableMultipleTouchProperty = serializedObject.FindProperty("enableMultipleTouch");
            _requireInternetProperty = serializedObject.FindProperty("requireInternet");
            _targetFrameRateProperty = serializedObject.FindProperty("targetFrameRate");
            _termOfServiceUrlProperty = serializedObject.FindProperty("termOfServiceUrl");
            _privacyUrlProperty = serializedObject.FindProperty("privacyUrl");
            _privacyTitleProperty = serializedObject.FindProperty("privacyTitle");
            _privacyMessageProperty = serializedObject.FindProperty("privacyMessage");
#if UNITY_IOS
            _appstoreAppIdProperty = serializedObject.FindProperty("appstoreAppId");
            _skAdConversionValueProperty = serializedObject.FindProperty("skAdConversionValue");
#endif
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUILayout.Label("[Runtime]".ToWhiteBold(), Uniform.RichLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_debugViewProperty, new GUIContent("Debug View"));
            EditorGUILayout.PropertyField(_enableMultipleTouchProperty, new GUIContent("Multiple Touch"));
            EditorGUILayout.PropertyField(_requireInternetProperty, new GUIContent("Require Internet"));
            EditorGUILayout.PropertyField(_targetFrameRateProperty, new GUIContent("Target Frame Rate"));

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(_termOfServiceUrlProperty, new GUIContent("Term of Service URL"));
                GUI.enabled = !string.IsNullOrEmpty(_termOfServiceUrlProperty.stringValue);
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    Application.OpenURL(_termOfServiceUrlProperty.stringValue);
                }

                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(_privacyUrlProperty, new GUIContent("Privacy URL"));
                GUI.enabled = !string.IsNullOrEmpty(_privacyUrlProperty.stringValue);
                if (GUILayout.Button("Open", GUILayout.Width(60)))
                {
                    Application.OpenURL(_privacyUrlProperty.stringValue);
                }

                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.PropertyField(_enablePrivacyFirstOpenProperty, new GUIContent("Privacy First Open"));
            if (_enablePrivacyFirstOpenProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_privacyTitleProperty, new GUIContent("Title"));
                EditorGUILayout.PropertyField(_privacyMessageProperty, new GUIContent("Message"));
                EditorGUI.indentLevel--;
            }

#if UNITY_IOS
            EditorGUILayout.PropertyField(_appstoreAppIdProperty, new GUIContent("Appstore App Id"));
            EditorGUILayout.PropertyField(_skAdConversionValueProperty, new GUIContent("SkAd Conversion Value"));
#endif
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }
    }
}