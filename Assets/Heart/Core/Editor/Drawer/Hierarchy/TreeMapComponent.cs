using Pancake.ExLibEditor;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    public class TreeMapComponent : BaseHierarchy
    {
        public TreeMapComponent()
        {
            rect.width = 14;
            rect.height = 16;
        }

        protected override bool Enabled => HierarchyEditorSetting.EnabledTreeMap;
        protected override bool ShowComponentDuringPlayMode => true;

        public override HierarchyLayoutStatus Layout(GameObject gameObject, Rect selectionRect, ref Rect currentRect, float maxWidth)
        {
            rect.y = selectionRect.y;
            if (!HierarchyEditorSetting.TreeMapTransparentBackground)
            {
                rect.x = 0;
                rect.width = selectionRect.x - 14;
                EditorGUI.DrawRect(rect, HierarchyEditorSetting.AdditionalBackgroundColor.Get());
                rect.width = 14;
            }

            return HierarchyLayoutStatus.Success;
        }

        public override void Draw(GameObject gameObject, Rect selectionRect)
        {
            int childCount = gameObject.transform.childCount;
            int level = Mathf.RoundToInt(selectionRect.x / 14f);

            if (HierarchyEditorSetting.TreeMapEnhanced)
            {
                var t = gameObject.transform;
                Transform parent = null;

                for (int i = 0, j = level - 1; j >= 0; i++, j--)
                {
                    rect.x = 14 * j;
                    if (i == 0)
                    {
                        if (childCount == 0)
                        {
                            GUI.color = HierarchyEditorSetting.TreeMapColor.Get();
                            GUI.DrawTexture(rect, EditorResources.TreeMapLine);
                        }

                        t = gameObject.transform;
                    }
                    else if (i == 1)
                    {
                        GUI.color = HierarchyEditorSetting.TreeMapColor.Get();
                        if (parent == null)
                        {
                            if (t.GetSiblingIndex() == gameObject.scene.rootCount - 1)
                            {
                                GUI.DrawTexture(rect, EditorResources.TreeMapLast);
                            }
                            else
                            {
                                GUI.DrawTexture(rect, EditorResources.TreeMapCurrent);
                            }
                        }
                        else if (t.GetSiblingIndex() == parent.childCount - 1)
                        {
                            GUI.DrawTexture(rect, EditorResources.TreeMapLast);
                        }
                        else
                        {
                            GUI.DrawTexture(rect, EditorResources.TreeMapCurrent);
                        }

                        t = parent;
                    }
                    else
                    {
                        if (parent == null)
                        {
                            if (t.GetSiblingIndex() != gameObject.scene.rootCount - 1) GUI.DrawTexture(rect, EditorResources.TreeMapLevel);
                        }
                        else if (t.GetSiblingIndex() != parent.childCount - 1) GUI.DrawTexture(rect, EditorResources.TreeMapLevel);

                        t = parent;
                    }

                    if (t != null) parent = t.parent;
                    else break;
                }

                GUI.color = Color.white;
            }
            else
            {
                for (int i = 0, j = level - 1; j >= 0; i++, j--)
                {
                    rect.x = 14 * j;
                    if (i == 0)
                    {
                        if (childCount > 0)
                            continue;
                        else
                        {
                            GUI.color = HierarchyEditorSetting.TreeMapColor.Get();
                            GUI.DrawTexture(rect, EditorResources.TreeMapLine);
                        }
                    }
                    else if (i == 1)
                    {
                        GUI.color = HierarchyEditorSetting.TreeMapColor.Get();
                        GUI.DrawTexture(rect, EditorResources.TreeMapCurrent);
                    }
                    else
                    {
                        rect.width = 14 * 4;
                        rect.x -= 14 * 3;
                        j -= 3;
                        GUI.DrawTexture(rect, EditorResources.TreeMapLevel4);
                        rect.width = 14;
                    }
                }

                GUI.color = Color.white;
            }
        }
    }
}