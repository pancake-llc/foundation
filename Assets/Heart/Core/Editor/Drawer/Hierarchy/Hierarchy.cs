using System.Collections.Generic;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    public class Hierarchy
    {
        private readonly List<BaseHierarchy> _collection = new() {new TreeMapComponent(), new SeparatorComponent()};

        public void OnHierarchyWindowItemOnGUI(GameObject gameObject, int instanceId, Rect selectionRect)
        {
            var curRect = new Rect(selectionRect);
            curRect.x += selectionRect.width;

            DrawComponents(_collection,
                selectionRect,
                ref curRect,
                gameObject,
                true,
                0);
        }


        private void DrawComponents(
            List<BaseHierarchy> components,
            Rect selectionRect,
            ref Rect rect,
            GameObject gameObject,
            bool drawBackground = false,
            float minX = 50)
        {
            if (Event.current.type == EventType.Repaint)
            {
                int toComponent = components.Count;
                HierarchyLayoutStatus layoutStatus = HierarchyLayoutStatus.Success;
                for (int i = 0, n = toComponent; i < n; i++)
                {
                    BaseHierarchy component = components[i];
                    if (component.IsEnabled())
                    {
                        layoutStatus = component.Layout(gameObject, selectionRect, ref rect, rect.x - minX);
                        if (layoutStatus != HierarchyLayoutStatus.Success)
                        {
                            toComponent = layoutStatus == HierarchyLayoutStatus.Failed ? i : i + 1;
                            rect.x -= 7;

                            break;
                        }
                    }
                    else
                    {
                        component.DisabledHandler(gameObject);
                    }
                }

                if (drawBackground)
                {
                    if (HierarchyEditorSetting.AdditionalBackgroundColor.Get().a != 0)
                    {
                        rect.width = selectionRect.x + selectionRect.width - rect.x /*- indentation*/;
                        EditorGUI.DrawRect(rect, HierarchyEditorSetting.AdditionalBackgroundColor.Get());
                    }

                    DrawComponents(_collection, selectionRect, ref rect, gameObject);
                }

                for (int i = 0, n = toComponent; i < n; i++)
                {
                    BaseHierarchy component = components[i];
                    if (component.IsEnabled())
                    {
                        component.Draw(gameObject, selectionRect);
                    }
                }

                if (layoutStatus != HierarchyLayoutStatus.Success)
                {
                    rect.width = 7;
                    GUI.DrawTexture(rect, EditorResources.IconTrim);
                }
            }
            else if (Event.current.isMouse)
            {
                for (int i = 0, n = components.Count; i < n; i++)
                {
                    BaseHierarchy component = components[i];
                    if (component.IsEnabled())
                    {
                        if (component.Layout(gameObject, selectionRect, ref rect, rect.x - minX) != HierarchyLayoutStatus.Failed)
                        {
                            component.EventHandler(gameObject, Event.current);
                        }
                    }
                }
            }
        }
    }
}