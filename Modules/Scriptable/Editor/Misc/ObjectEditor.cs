using UnityEditor;
using Object = UnityEngine.Object;

namespace Obvious.Soap.Editor
{
    [CustomEditor(typeof(Object), true)]
    [CanEditMultipleObjects]
    internal class ObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
