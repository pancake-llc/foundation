using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PancakeEditor.ContextMenu
{
    [CustomEditor(typeof(HierarchyMenuSettings))]
    internal sealed class HierarchyMenuSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _property;
        private ReorderableList _reorderableList;

        private void OnEnable()
        {
            _property = serializedObject.FindProperty("lists");
            _reorderableList = new ReorderableList(serializedObject, _property) {elementHeight = 44, drawElementCallback = OnDrawElement};
        }

        private void OnDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _property.GetArrayElementAtIndex(index);
            rect.height -= 4;
            rect.y += 2;
            EditorGUI.PropertyField(rect, element);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _reorderableList.DoLayoutList();

            if (GUILayout.Button("Reset"))
            {
                Undo.RecordObject(target, "Reset");
                var settings = target as HierarchyMenuSettings;
                settings.Reset();
            }

            if (GUILayout.Button("Reset To Default"))
            {
                Undo.RecordObject(target, "Reset To Default");
                var settings = target as HierarchyMenuSettings;
                if (settings != null) settings.ResetToDefault();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}