using Pancake.Tag;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Pancake.TagEditor
{
    [CustomEditor(typeof(TagGameObject), true)]
    [CanEditMultipleObjects]
    public class TagGameObjectEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            root.Add(new PropertyField(serializedObject.FindProperty("tag")));
            return root;
        }
    }
}