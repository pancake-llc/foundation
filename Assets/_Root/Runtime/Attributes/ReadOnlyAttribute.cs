using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
#endif

namespace Pancake
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute
    {
#if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
        private class ReadOnlyDrawer : BasePropertyDrawer<ReadOnlyAttribute>
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                bool previousState = GUI.enabled;
                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = previousState;
            }
        }
#endif
    }
}