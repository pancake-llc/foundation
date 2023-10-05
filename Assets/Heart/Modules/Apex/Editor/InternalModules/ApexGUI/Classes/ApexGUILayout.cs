using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Miscellaneous helper stuff for layout control editable visual entities.
    /// </summary>
    public static class ApexGUILayout
    {
        /// <summary>
        /// Reserve control layout space for a rectangle with a specified height.
        /// </summary>
        /// <param name="height">Rectangle height.</param>
        public static Rect GetControlRect(float height)
        {
            Rect position = GUILayoutUtility.GetRect(EditorGUIUtility.fieldWidth, height + EditorGUIUtility.standardVerticalSpacing);
            position.height = height;
            return position;
        }
    }
}