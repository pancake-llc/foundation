using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Miscellaneous helper stuff for control editable visual entities.
    /// </summary>
    public static class ApexGUI
    {
        /// <summary>
        /// The indent level of the editable controls.
        /// </summary>
        public static int IndentLevel { get; set; } = 0;

        /// <summary>
        /// The indent level space of the editable controls.
        /// </summary>
        public static float IndentLevelSpace { get; internal set; } = 15.0f;

        /// <summary>
        /// Ignore indent level for the next editable controls.
        /// </summary>
        public static bool IgnoreIndentLevel { get; set; } = false;

        /// <summary>
        /// Apply current indent level for rectangle.
        /// </summary>
        /// <param name="position">Source rectangle position.</param>
        public static void IndentedRect(ref Rect position)
        {
            if (!IgnoreIndentLevel)
            {
                float indent = IndentLevel * IndentLevelSpace;
                position.x += indent;
                position.width -= indent;
            }
        }

        /// <summary>
        /// Apply current indent level for rectangle.
        /// </summary>
        /// <param name="position">Source rectangle position.</param>
        public static void RemoveIndentFromRect(ref Rect position)
        {
            if (!IgnoreIndentLevel && IndentLevel > 0)
            {
                float indent = IndentLevel * IndentLevelSpace;
                position.x -= indent;
                position.width += indent;
            }
        }

        /// <summary>
        /// Back to return current rectangle from indent level.
        /// </summary>
        /// <param name="position">Source rectangle position.</param>
        public static Rect InverseIndentedRect(Rect position)
        {
            if (!IgnoreIndentLevel)
            {
                float indent = IndentLevel * IndentLevelSpace;
                position.x -= indent;
                position.width += indent;
            }

            return position;
        }

        /// <summary>
        /// Makes a label with a foldout arrow to the left of it. 
        /// This is useful for creating tree or folder like structures where child objects are only shown if the parent is folded out.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the arrow and label.</param>
        /// <param name="isExpanded">The shown foldout state.</param>
        /// <param name="label">The label to show.</param>
        /// <returns>The foldout state selected by the user. If true, you should render sub-objects.</returns>
        public static bool Foldout(Rect position, bool isExpanded, string label)
        {
            Event current = Event.current;
            bool isHover = position.Contains(current.mousePosition);
            if (current.type == EventType.MouseDown)
            {
                if (isHover)
                {
                    isExpanded = !isExpanded;
                }
            }
            else if (current.type == EventType.Repaint)
            {
                position.x -= 2;
                EditorStyles.foldout.Draw(position,
                    label,
                    isHover,
                    false,
                    isExpanded,
                    false);
            }

            return isExpanded;
        }

        /// <summary>
        /// Makes a label with a foldout arrow to the left of it. 
        /// This is useful for creating tree or folder like structures where child objects are only shown if the parent is folded out.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the arrow and label.</param>
        /// <param name="isExpanded">The shown foldout state.</param>
        /// <param name="label">The label to show.</param>
        /// <returns>The foldout state selected by the user. If true, you should render sub-objects.</returns>
        public static bool Foldout(Rect position, bool isExpanded, GUIContent label) { return Foldout(position, isExpanded, label.text); }
    }
}