using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public class Field
    {
        public SerializedProperty property;
        public GUIContent content;

        public Field(SerializedProperty property, GUIContent content)
        {
            this.property = property;
            this.content = content;
        }

        public Field(GUIContent content) { this.content = content; }
    }
}