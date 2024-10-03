using System;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    // ReSharper disable once UnusedType.Global
    public class HierarchyRowSeparatorDrawer : HierarchyDrawer
    {
        public override void OnGUI(int instanceID, Rect selectionRect)
        {
            try
            {
                _ = HierarchySettings.Instance;
            }
            catch (Exception)
            {
                return;
            }

            if (!HierarchySettings.ShowSeparator) return;
            var rect = new Rect {y = selectionRect.y, width = selectionRect.width + selectionRect.x, height = 1, x = 0};

            EditorGUI.DrawRect(rect, HierarchySettings.SeparatorColor);

            if (!HierarchySettings.ShowRowShading) return;
            selectionRect.width += selectionRect.x;
            selectionRect.x = 0;
            selectionRect.height -= 1;
            selectionRect.y += 1;
            EditorGUI.DrawRect(selectionRect, Mathf.FloorToInt((selectionRect.y - 4) / 16 % 2) == 0 ? HierarchySettings.EvenRowColor : HierarchySettings.OddRowColor);
        }
    }
}