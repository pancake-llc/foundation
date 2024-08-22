using PancakeEditor.Common;
using Pancake.UI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.UI
{
    [CustomEditor(typeof(UIButtonText), true)]
    [CanEditMultipleObjects]
    public class UIButtonTextEditor : UIButtonEditor
    {
        private SerializedProperty _label;

        protected override void DrawInspector()
        {
            _label = serializedObject.FindProperty("label");

            Uniform.DrawGroupFoldout("uibutton_setting",
                "Settings",
                () => Draw(() =>
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("  Label"), GUILayout.Width(DEFAULT_LABEL_WIDTH));
                    EditorGUILayout.PropertyField(_label, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                }));
        }
    }
}