using Pancake.SignIn;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.SignIn
{
    [CustomEditor(typeof(GameServiceSettings), true)]
    public class GameServiceSettingDrawer : Editor
    {
        private SerializedProperty _enableAutoBackupProperty;
        private SerializedProperty _byTimeProperty;
        private SerializedProperty _byApplicationQuitProperty;
        private SerializedProperty _backupTimeIntervalProperty;

        private void OnEnable()
        {
            _enableAutoBackupProperty = serializedObject.FindProperty("enableAutoBackup");
            _byTimeProperty = serializedObject.FindProperty("byTime");
            _byApplicationQuitProperty = serializedObject.FindProperty("byApplicationQuit");
            _backupTimeIntervalProperty = serializedObject.FindProperty("backupTimeInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_enableAutoBackupProperty, new GUIContent("Enable Auto Backup"));
            if (_enableAutoBackupProperty.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox("Backup data at regular intervals", MessageType.None);
                EditorGUILayout.PropertyField(_byTimeProperty, new GUIContent("By Time"));
                if (_byTimeProperty.boolValue) EditorGUILayout.PropertyField(_backupTimeIntervalProperty, new GUIContent("Time Interval (second)"));
                EditorGUILayout.HelpBox("Backup data every time you exit the application", MessageType.None);
                EditorGUILayout.PropertyField(_byApplicationQuitProperty, new GUIContent("When Application Quit"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}