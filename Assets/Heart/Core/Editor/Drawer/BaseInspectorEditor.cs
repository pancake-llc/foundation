using System;
using Pancake.Common;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    public class BaseInspectorEditor : UnityEditor.Editor
    {
        public static Action afterModifiedPropertyAction;
        private bool _isMissingScript;

        public override void OnInspectorGUI()
        {
            if (_isMissingScript)
            {
                OnInspectorMissingScript();
                return;
            }

            serializedObject.UpdateIfRequiredOrScript();

            if (serializedObject.ApplyModifiedProperties()) C.CallActionClean(ref afterModifiedPropertyAction);
        }


        private void OnInspectorMissingScript()
        {
            Uniform.SetGUIEnabled(true);
            if (serializedObject.FindProperty("m_Script") is { } scriptProperty)
            {
                EditorGUILayout.PropertyField(scriptProperty);
                serializedObject.ApplyModifiedProperties();
            }
            
            const string message = @"Script cannot be loaded
Possible reasons:
- Compile erros
- Script is deleted
- Script file name doesn't match class name
- Class doesn't inherit from MonoBehaviour";
            Uniform.Space();
            EditorGUILayout.HelpBox(message, MessageType.Warning, true);
            Uniform.Space();
            Uniform.ResetGUIEnabled();
        }

        private void OnEnable()
        {
            if (target) _isMissingScript = target.GetType() == typeof(MonoBehaviour) || target.GetType() == typeof(ScriptableObject);
            else _isMissingScript = target is MonoBehaviour or ScriptableObject;

            if (_isMissingScript) return;
        }
    }
}