using System;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    // ReSharper disable once UnusedType.Global
    public class HierarchyTreeMapDrawer : HierarchyDrawer
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
            
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null) return;

            var tempColor = GUI.color;

            if (HierarchySettings.ShowTreeMap)
            {
                selectionRect.width = 14;
                selectionRect.height = 16;

                int childCount = gameObject.transform.childCount;
                int level = Mathf.RoundToInt(selectionRect.x / 14f);
                var t = gameObject.transform;
                Transform parent = null;

                for (int i = 0, j = level - 1; j >= 0; i++, j--)
                {
                    selectionRect.x = 14 * j;
                    if (i == 0)
                    {
                        if (childCount == 0)
                        {
                            GUI.color = HierarchySettings.TreeMapColor;
                            GUI.DrawTexture(selectionRect, EditorResources.IconTreeMapLine);
                        }

                        t = gameObject.transform;
                    }
                    else if (i == 1)
                    {
                        GUI.color = HierarchySettings.TreeMapColor;
                        if (parent == null)
                        {
                            if (t.GetSiblingIndex() == gameObject.scene.rootCount - 1)
                            {
                                GUI.DrawTexture(selectionRect, EditorResources.IconTreeMapLast);
                            }
                            else
                            {
                                GUI.DrawTexture(selectionRect, EditorResources.IconTreeMapCurrent);
                            }
                        }
                        else if (t.GetSiblingIndex() == parent.childCount - 1)
                        {
                            GUI.DrawTexture(selectionRect, EditorResources.IconTreeMapLast);
                        }
                        else
                        {
                            GUI.DrawTexture(selectionRect, EditorResources.IconTreeMapCurrent);
                        }

                        t = parent;
                    }
                    else
                    {
                        if (parent == null)
                        {
                            if (t.GetSiblingIndex() != gameObject.scene.rootCount - 1) GUI.DrawTexture(selectionRect, EditorResources.IconTreeMapLevel);
                        }
                        else if (t.GetSiblingIndex() != parent.childCount - 1) GUI.DrawTexture(selectionRect, EditorResources.IconTreeMapLevel);

                        t = parent;
                    }

                    if (t != null) parent = t.parent;
                    else break;
                }

                GUI.color = tempColor;
            }
        }
    }
}