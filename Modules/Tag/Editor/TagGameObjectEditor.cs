using Pancake.BTag;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Pancake.BTagEditor
{
    [CustomEditor(typeof(TagGameObject), true)]
    [CanEditMultipleObjects]
    public class BTagGameObjectEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new PropertyField(serializedObject.FindProperty("tag")));
            return root;
        }
    }
}