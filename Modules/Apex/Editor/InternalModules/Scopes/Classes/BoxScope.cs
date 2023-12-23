using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public sealed class BoxScope : GUI.Scope
    {
        private readonly bool labelWidth;

        public BoxScope(ref Rect position, bool labelWidth = true)
        {
            position.x += ApexGUIUtility.BoxBounds;
            position.width -= ApexGUIUtility.BoxBounds * 2;

            this.labelWidth = labelWidth;
            if (labelWidth)
            {
                EditorGUIUtility.labelWidth -= ApexGUIUtility.BoxBounds;
            }
        }

        protected override void CloseScope()
        {
            if (labelWidth)
            {
                EditorGUIUtility.labelWidth += ApexGUIUtility.BoxBounds;
            }
        }
    }
}