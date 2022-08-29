using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.UI.Editor
{
    public static class Align
    {
        private static AlignMode alignMode;
        private static AlignTo alignTo;

        public static void AlignSelection(AlignMode alignMode, AlignTo alignTo)
        {
            Align.alignMode = alignMode;
            Align.alignTo = alignTo;

            Transform[] selection = Selection.GetTransforms(SelectionMode.TopLevel);

            RectTransform targetRectTransform = GetAlignTargetTransform(selection);

            //Rect targetRect = targetRectTransform.rect;
            // Debug.Log("[Align]\tTarget: \t" + targetRectTransform.name + "\n\tMode: \t" + alignMode);

            for (int i = 0; i < selection.Length; i++)
            {
                RectTransform rectTransform = (RectTransform)selection[i].transform;
                Undo.RecordObject(rectTransform, "Align " + alignTo);

                Vector3 oldPosition = rectTransform.position;
                Quaternion originalParentRotation = rectTransform.parent.rotation;
                rectTransform.parent.rotation = new Quaternion();
                rectTransform.position = GetAlignTargetPosition(rectTransform, targetRectTransform);
                rectTransform.parent.rotation = originalParentRotation;

                Utility.AdjustAnchors(rectTransform, oldPosition);
            }

            Utility.CleanUp();
        }

        private static RectTransform GetAlignTargetTransform(Transform[] selection)
        {
            switch (alignTo)
            {
                case AlignTo.Parent:
                    return selection[0].parent as RectTransform;

                case AlignTo.FirstInHierarchy:
                    return Utility.SortHierarchically(selection)[0] as RectTransform;

                case AlignTo.LastInHierarchy:
                    return Utility.SortHierarchically(selection)[selection.Length - 1] as RectTransform;

                case AlignTo.SmallestObject:
                    if (alignMode == AlignMode.Left || alignMode == AlignMode.Vertical || alignMode == AlignMode.Right)
                    {
                        return Utility.SortByWidth(selection)[0] as RectTransform;
                    }
                    else
                    {
                        return Utility.SortByHeight(selection)[0] as RectTransform;
                    }

                case AlignTo.BiggestObject:
                    if (alignMode == AlignMode.Left || alignMode == AlignMode.Vertical || alignMode == AlignMode.Right)
                    {
                        return Utility.SortByWidth(selection)[selection.Length - 1] as RectTransform;
                    }
                    else
                    {
                        return Utility.SortByHeight(selection)[selection.Length - 1] as RectTransform;
                    }

                case AlignTo.SelectionBounds:
                    return Utility.GetBoundingBoxRectTransform(selection);

                default:
                    Debug.LogError("Unknown AlignTo: " + alignTo);
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Vector3 GetAlignTargetPosition(RectTransform currentRectTransform, RectTransform targetRectTransform)
        {
            Vector3 position = currentRectTransform.position;

            if (currentRectTransform == targetRectTransform)
            {
                return position;
            }

            Vector2 currentSize = Utility.GetTransformSize(currentRectTransform);
            Vector2 targetSize = Utility.GetTransformSize(targetRectTransform);

            Vector2 currentPivotLocal = Utility.GetPivotAndCenterLocalDistance(currentRectTransform) + currentSize * 0.5f;
            Vector2 targetPivotLocal = Utility.GetPivotAndCenterLocalDistance(targetRectTransform) + targetSize * 0.5f;

            switch (alignMode)
            {
                case AlignMode.Left:
                    position.x = currentPivotLocal.x + targetRectTransform.position.x - targetPivotLocal.x;
                    break;

                case AlignMode.Right:
                    position.x = targetSize.x - currentSize.x + currentPivotLocal.x
                               + targetRectTransform.position.x - targetPivotLocal.x;
                    break;

                case AlignMode.Vertical:
                    position.x = Utility.GetPivotAndCenterLocalDistance(currentRectTransform).x
                               + targetRectTransform.position.x - Utility.GetPivotAndCenterLocalDistance(targetRectTransform).x;
                    break;

                case AlignMode.Bottom:
                    position.y = currentPivotLocal.y + targetRectTransform.position.y - targetPivotLocal.y;
                    break;

                case AlignMode.Top:
                    position.y = targetSize.y - currentSize.y + currentPivotLocal.y
                               + targetRectTransform.position.y - targetPivotLocal.y;
                    break;

                case AlignMode.Horizontal:
                    position.y = Utility.GetPivotAndCenterLocalDistance(currentRectTransform).y
                               + targetRectTransform.position.y - Utility.GetPivotAndCenterLocalDistance(targetRectTransform).y;
                    break;

                default:
                    Debug.LogError("Unknown AlignMode: " + alignMode);
                    throw new ArgumentOutOfRangeException();
            }

            return position;
        }

    }
}