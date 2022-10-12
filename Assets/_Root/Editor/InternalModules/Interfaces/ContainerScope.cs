using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public sealed class ContainerScope : GUI.Scope
    {
        public static int indent = -1;

        public ContainerScope(ref Rect position)
        {
            indent = EditorStyles.foldout.padding.left - EditorStyles.label.padding.left;
            if (EditorGUIUtility.hierarchyMode)
            {
                position.xMin -= indent;
                EditorGUIUtility.labelWidth += indent;
            }
        }

        protected override void CloseScope()
        {
            if (EditorGUIUtility.hierarchyMode)
            {
                EditorGUIUtility.labelWidth -= indent;
            }
        }
    }
}