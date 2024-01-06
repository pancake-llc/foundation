using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    public class SeparatorComponent : BaseHierarchy
    {
        protected override bool Enabled => HierarchyEditorSetting.EnabledSeparator;

        public override void Draw(GameObject gameObject, Rect selectionRect)
        {
            rect.y = selectionRect.y;
            rect.width = selectionRect.width + selectionRect.x;
            rect.height = 1;
            rect.x = 0;

            EditorGUI.DrawRect(rect, HierarchyEditorSetting.SeperatorColor.Get());

            if (HierarchyEditorSetting.ShowRowShading)
            {
                selectionRect.width += selectionRect.x;
                selectionRect.x = 0;
                selectionRect.height -= 1;
                selectionRect.y += 1;
                EditorGUI.DrawRect(selectionRect,
                    ((Mathf.FloorToInt(((selectionRect.y - 4) / 16) % 2) == 0)) ? HierarchyEditorSetting.EvenRowColor.Get() : HierarchyEditorSetting.OddRowColor.Get());
            }
        }
    }
}