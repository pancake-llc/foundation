using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    /// <summary>
    /// Base class for adding custom drawing processing to hierarchy items.
    /// </summary>
    public abstract class HierarchyDrawer
    {
        public abstract void OnGUI(int instanceID, Rect selectionRect);

        protected static Rect GetBackgroundRect(Rect selectionRect) { return selectionRect.AddXMax(20f); }

        protected static void DrawBackground(int instanceID, Rect selectionRect)
        {
            var backgroundRect = GetBackgroundRect(selectionRect);

            Color backgroundColor;
            var e = Event.current;
            var isHover = backgroundRect.Contains(e.mousePosition);

            if (Selection.Contains(instanceID)) backgroundColor = Uniform.HighlightBackground;
            else if (isHover) backgroundColor = Uniform.HighlightBackgroundInactive;
            else backgroundColor = Uniform.WindowBackground;

            EditorGUI.DrawRect(backgroundRect, backgroundColor);
        }
    }
}