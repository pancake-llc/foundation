﻿using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Pancake.Editor;
#endif

namespace Pancake
{
    /// <summary>
    /// set indent of a field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class IndentAttribute : PropertyAttribute
    {
        private int _indentLevel;

        public IndentAttribute(int indentLevel = 1) { _indentLevel = indentLevel; }

#if UNITY_EDITOR

        [CustomPropertyDrawer(typeof(IndentAttribute))]
        private class IndentDrawer : BasePropertyDrawer<IndentAttribute>
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.indentLevel += attribute._indentLevel;
                base.OnGUI(position, property, label);
                EditorGUI.indentLevel -= attribute._indentLevel;
            }
        }

#endif // UNITY_EDITOR
    } // class IndentAttribute
} // namespace Pancake