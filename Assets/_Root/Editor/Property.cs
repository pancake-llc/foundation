using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class Property
    {
        public SerializedProperty property;
        public GUIContent content;

        public Property(SerializedProperty property, GUIContent content)
        {
            this.property = property;
            this.content = content;
        }

        public Property(GUIContent content) { this.content = content; }
    }
}