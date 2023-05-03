using Pancake.Scriptable;
using UnityEditor;


namespace Pancake.ScriptableEditor
{
    [CustomEditor(typeof(ScriptableBase), true)]
    [CanEditMultipleObjects]
    internal class ObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() { DrawDefaultInspector(); }
    }
}