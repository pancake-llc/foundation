using Pancake.Scriptable;
using UnityEditor;


namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(ScriptableBase), true)]
    [CanEditMultipleObjects]
    internal class ObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var targetType = serializedObject.targetObject.GetType();
            var customType = typeof(CustomEditor);
            object[] customEditors = targetType.GetCustomAttributes(customType, true);

            if (customEditors.Length == 0)
            {
                DrawDefaultInspector();
            }
            else
            {
                // Custom editor exists, handle it accordingly
                base.OnInspectorGUI();
            }
        }
    }
}