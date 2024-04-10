using System.Collections.Generic;
using Pancake;
using Pancake.Common;
using PancakeEditor.Common;

using UnityEditor;
using UnityEngine;

namespace PancakeEditor.Hierarchy
{
    public class VisibilityComponent : BaseHierarchy
    {
        private int _targetVisibilityState = -1;

        public VisibilityComponent() { rect.width = 18; }

        protected override bool Enabled => HierarchyEditorSetting.EnabledVisibility;
        protected override bool ShowComponentDuringPlayMode => HierarchyEditorSetting.VisibilityShowDuringPlayMode;

        public override HierarchyLayoutStatus Layout(GameObject gameObject, Rect selectionRect, ref Rect currentRect, float maxWidth)
        {
            if (maxWidth < 18) return HierarchyLayoutStatus.Failed;

            rect.x = currentRect.x;
            rect.y = currentRect.y;
            return HierarchyLayoutStatus.Success;
        }

        public override void Draw(GameObject gameObject, Rect selectionRect)
        {
            int visibility = gameObject.activeSelf ? 1 : 0;

            var transform = gameObject.transform;
            while (transform.parent != null)
            {
                transform = transform.parent;
                if (!transform.gameObject.activeSelf)
                {
                    visibility = 2;
                    break;
                }
            }

            if (visibility == 0)
            {
                GUI.color = HierarchyEditorSetting.AdditionalInactiveColor.Get();
                GUI.DrawTexture(rect, EditorResources.IconEyeClose);
            }
            else if (visibility == 1)
            {
                GUI.color = HierarchyEditorSetting.AdditionalActiveColor.Get();
                GUI.DrawTexture(rect, EditorResources.IconEyeOpen);
            }
            else
            {
                if (gameObject.activeSelf)
                {
                    GUI.color = HierarchyEditorSetting.AdditionalActiveColor.Get().Mul(0.65f);
                    GUI.DrawTexture(rect, EditorResources.IconEyeOpen);
                }
                else
                {
                    GUI.color = HierarchyEditorSetting.AdditionalInactiveColor.Get().Mul(0.85f);
                    GUI.DrawTexture(rect, EditorResources.IconEyeClose);
                }
            }

            GUI.color = Color.white;
        }

        public override void EventHandler(GameObject gameObject, Event currentEvent)
        {
            if (currentEvent.isMouse && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.type == EventType.MouseDown)
                {
                    _targetVisibilityState = !gameObject.activeSelf ? 1 : 0;
                }
                else if (currentEvent.type == EventType.MouseDrag && _targetVisibilityState != -1)
                {
                    if (_targetVisibilityState == (gameObject.activeSelf ? 1 : 0)) return;
                }
                else
                {
                    _targetVisibilityState = -1;
                    return;
                }

                var targetGameObjects = new List<GameObject>();
                if (currentEvent.control || currentEvent.command)
                {
                    if (currentEvent.shift)
                    {
                        if (!HierarchyEditorSetting.AdditionalShowModifierWarning || EditorUtility.DisplayDialog("Change edit-time visibility",
                                "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") +
                                " the edit-time visibility of this GameObject and all its children? (You can disable this warning in the settings)",
                                "Yes",
                                "Cancel"))
                        {
                            GetListGameObjectRecursive(gameObject, ref targetGameObjects);
                        }
                    }
                    else if (currentEvent.alt)
                    {
                        if (gameObject.transform.parent != null)
                        {
                            if (!HierarchyEditorSetting.AdditionalShowModifierWarning || EditorUtility.DisplayDialog("Change edit-time visibility",
                                    "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") +
                                    " the edit-time visibility this GameObject and its siblings? (You can disable this warning in the settings)",
                                    "Yes",
                                    "Cancel"))
                            {
                                var parent = gameObject.transform.parent;
                                GetListGameObjectRecursive(parent.gameObject, ref targetGameObjects, 1);
                                targetGameObjects.Remove(parent.gameObject);
                            }
                        }
                        else
                        {
                            Debug.Log("This action for root objects is supported for Unity3d 5.3.3 and above");
                            return;
                        }
                    }
                    else
                    {
                        GetListGameObjectRecursive(gameObject, ref targetGameObjects, 0);
                    }
                }
                else if (currentEvent.shift)
                {
                    if (!HierarchyEditorSetting.AdditionalShowModifierWarning || EditorUtility.DisplayDialog("Change visibility",
                            "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") +
                            " the visibility of this GameObject and all its children? (You can disable this warning in the settings)",
                            "Yes",
                            "Cancel"))
                    {
                        GetListGameObjectRecursive(gameObject, ref targetGameObjects);
                    }
                }
                else if (currentEvent.alt)
                {
                    if (gameObject.transform.parent != null)
                    {
                        if (!HierarchyEditorSetting.AdditionalShowModifierWarning || EditorUtility.DisplayDialog("Change visibility",
                                "Are you sure you want to turn " + (gameObject.activeSelf ? "off" : "on") +
                                " the visibility this GameObject and its siblings? (You can disable this warning in the settings)",
                                "Yes",
                                "Cancel"))
                        {
                            var parent = gameObject.transform.parent;
                            GetListGameObjectRecursive(parent.gameObject, ref targetGameObjects, 1);
                            targetGameObjects.Remove(parent.gameObject);
                        }
                    }
                    else
                    {
                        Debug.Log("This action for root objects is supported for Unity3d 5.3.3 and above");
                        return;
                    }
                }
                else
                {
                    if (Selection.Contains(gameObject))
                    {
                        targetGameObjects.AddRange(Selection.gameObjects);
                    }
                    else
                    {
                        GetListGameObjectRecursive(gameObject, ref targetGameObjects, 0);
                    }
                }

                SetVisibility(targetGameObjects, !gameObject.activeSelf);
                currentEvent.Use();
            }
        }

        private void SetVisibility(List<GameObject> gameObjects, bool targetVisibility)
        {
            if (gameObjects.Count == 0) return;

            for (int i = gameObjects.Count - 1; i >= 0; i--)
            {
                var curGameObject = gameObjects[i];
                Undo.RecordObject(curGameObject, "visibility change");

                curGameObject.SetActive(targetVisibility);
                EditorUtility.SetDirty(curGameObject);
            }
        }
    }
}