using System;
using UnityEditor;
using UnityEngine;

namespace Pancake.UI.Editor
{
    public static class Distribute
    {
        private static AlignMode alignMode;
        private static DistanceOption distanceOption;
        private static SortOrder sortOrder;

        private static float paddingLeftBottomPixels;
        private static float paddingRightTopPixels;

        public static void DistributeSelection(AlignMode alignMode, DistanceOption distanceOption, SortOrder sortOrder, float paddingLeftBottomPixels, float paddingRightTopPixels)
        {
            Distribute.alignMode = alignMode;
            Distribute.distanceOption = distanceOption;
            Distribute.sortOrder = sortOrder;

            Distribute.paddingLeftBottomPixels = paddingLeftBottomPixels;
            Distribute.paddingRightTopPixels = paddingRightTopPixels;

            Transform[] selection = GetSortedSelection();
            RectTransform targetRectTransform = GetTargetRectTransform(selection);

            Vector2 start = CalculateStart(targetRectTransform, selection[0] as RectTransform);
            Vector2 end = CalculateEnd(targetRectTransform, selection[selection.Length - 1].transform as RectTransform);

            Vector2 distanceBetweenElements = CalculateDistanceBetweenElements(start, end, selection);

            // Debug.Log("[Distribute]\tTarget: \t" + alignMode + "\n\t\tMode:\t" + distanceOption);

            Vector2 currentPosition = start;
            for (int i = 0; i < selection.Length; i++)
            {
                Undo.RecordObject(selection[i].transform, "Distribute " + distanceOption);

                // DrawDebugLines(currentPosition, selection[i]);

                Vector3 oldPosition = selection[i].position;
                Quaternion originalParentRotation = selection[i].parent.rotation;
                selection[i].parent.rotation = new Quaternion();

                Vector3 targetPosition = selection[i].position;
                switch (alignMode)
                {
                    case AlignMode.Horizontal:
                        targetPosition.x = currentPosition.x + GetDistributionOffset(selection[i] as RectTransform).x;
                        break;

                    case AlignMode.Vertical:
                        targetPosition.y = currentPosition.y + GetDistributionOffset(selection[i] as RectTransform).y;
                        break;

                    default:
                        Debug.LogError("Invalid target: " + alignMode);
                        break;
                }
                selection[i].position = targetPosition;
                selection[i].parent.rotation = originalParentRotation;

                Utility.AdjustAnchors(selection[i] as RectTransform, oldPosition);

                if (distanceOption == DistanceOption.Space)
                {
                    currentPosition += Utility.GetTransformSize(selection[i]);
                }

                currentPosition += distanceBetweenElements;
            }

            Utility.CleanUp();
        }

        private static RectTransform GetTargetRectTransform(Transform[] selection)
        {
            if (AlignDistributeWindow.distributeTo == DistributeTo.Parent)
            {
                return selection[0].parent as RectTransform;
            }
            else
            {
                return Utility.GetBoundingBoxRectTransform(selection);
            }
        }

        private static Vector2 CalculateStart(RectTransform targetRectTransform, RectTransform firstSelectionElement)
        {
            Vector2 start = (Vector2) targetRectTransform.position - Utility.GetTransformSize(targetRectTransform) * 0.5f;
            start += Vector2.one * paddingLeftBottomPixels;
            start -= Utility.GetPivotAndCenterLocalDistance(targetRectTransform);

            if (distanceOption == DistanceOption.Space)
            {
                return start;
            }

            // Make sure the first object never overlaps the Parent's borders.
            start += Utility.GetTransformSize(firstSelectionElement) * 0.5f;
            start += Utility.GetPivotAndCenterLocalDistance(firstSelectionElement);
            start -= GetDistributionOffset(firstSelectionElement);

            return start;
        }

        private static Vector2 CalculateEnd(RectTransform targetRectTransform, RectTransform lastSelectionElement)
        {
            Vector2 end = (Vector2) targetRectTransform.position + Utility.GetTransformSize(targetRectTransform) * 0.5f;
            end -= Vector2.one * paddingRightTopPixels;
            end -= Utility.GetPivotAndCenterLocalDistance(targetRectTransform);

            if (distanceOption == DistanceOption.Space)
            {
                return end;
            }

            // Make sure the last object never overlaps the Parent's borders.
            end -= Utility.GetTransformSize(lastSelectionElement) * 0.5f;
            end += Utility.GetPivotAndCenterLocalDistance(lastSelectionElement);
            end -= GetDistributionOffset(lastSelectionElement);

            return end;
        }

        private static Transform[] GetSortedSelection()
        {
            switch (sortOrder)
            {
                case SortOrder.Hierarchical:
                    return Utility.SortHierarchically(Selection.transforms);

                case SortOrder.Positional:
                    if (alignMode == AlignMode.Horizontal)
                    {
                        return Utility.SortByPositionX(Selection.transforms);
                    }
                    else
                    {
                        return Utility.SortByPositionY(Selection.transforms);
                    }

                default:
                    Debug.LogError("Unknown SortOrder: " + sortOrder);
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static Vector2 CalculateDistanceBetweenElements(Vector2 start, Vector2 end, Transform[] elements)
        {
            Vector2 totalLength = end - start;
            totalLength.x = Mathf.Abs(totalLength.x);
            totalLength.y = Mathf.Abs(totalLength.y);

            Vector2 totalElementSize = Vector2.zero;
            if (distanceOption == DistanceOption.Space)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    totalElementSize += Utility.GetTransformSize(elements[i]);
                }
            }

            return (totalLength - totalElementSize) / (elements.Length - 1);
        }

        private static Vector2 GetDistributionOffset(RectTransform rectTransform)
        {
            Vector2 offset = Vector2.zero;

            switch (distanceOption)
            {
                case DistanceOption.Center:
                    offset = Utility.GetPivotAndCenterLocalDistance(rectTransform);
                    break;

                case DistanceOption.Pivot:
                    break;

                case DistanceOption.LeftBottom:
                    offset = Utility.GetLocalPivotPosition(rectTransform);
                    break;

                case DistanceOption.RightTop:
                    offset = Utility.GetLocalPivotPosition(rectTransform) - Utility.GetTransformSize(rectTransform);
                    break;

                case DistanceOption.Space:
                    offset = Utility.GetLocalPivotPosition(rectTransform);
                    break;

                default:
                    Debug.LogError("Unknown DistanceOption: " + distanceOption);
                    throw new ArgumentOutOfRangeException();
            }

            return offset;
        }

        private static void DrawDebugLines(Vector2 currentPosition, Transform currentSelection)
        {
            // CURRENT POS DEBUG
            Debug.DrawLine(currentPosition + Vector2.up * 100, currentPosition + Vector2.down * 100, Color.yellow, 1f);
            Debug.DrawLine(currentPosition + Vector2.right * 100, currentPosition + Vector2.left * 100, Color.yellow, 1f);

            // DEBUG PIVOT
            Debug.DrawLine(currentSelection.position + Vector3.up, currentSelection.position + Vector3.down, Color.cyan, 1f);
            Debug.DrawLine(currentSelection.position + Vector3.up, currentSelection.position + Vector3.left, Color.cyan, 1f);
            Debug.DrawLine(currentSelection.position + Vector3.up, currentSelection.position + Vector3.right, Color.cyan, 1f);

            Debug.DrawLine(currentSelection.position + Vector3.down, currentSelection.position + Vector3.left, Color.cyan, 1f);
            Debug.DrawLine(currentSelection.position + Vector3.down, currentSelection.position + Vector3.right, Color.cyan, 1f);
            Debug.DrawLine(currentSelection.position + Vector3.left, currentSelection.position + Vector3.right, Color.cyan, 1f);

            SceneView.RepaintAll();
        }
    }
}