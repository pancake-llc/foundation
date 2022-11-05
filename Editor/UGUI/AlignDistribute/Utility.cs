using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake.UI.Editor
{
    public static class Utility
    {
        internal static GameObject helperObject;

        internal static void CleanUp()
        {
            if (helperObject != null)
            {
                Object.DestroyImmediate(helperObject);
            }
        }

        // Sort from smallest to largest
        private class AreaComparer : IComparer<Transform>
        {
            public int Compare(Transform a, Transform b) { return GetTransformArea(a).CompareTo(GetTransformArea(b)); }

            private float GetTransformArea(Transform transform)
            {
                Vector2 size = GetTransformSize(transform);
                return size.x * size.y;
            }
        }

        private class WidthComparer : IComparer<Transform>
        {
            public int Compare(Transform a, Transform b)
            {
                Vector2 sizeA = GetTransformSize(a);
                Vector2 sizeB = GetTransformSize(b);
                return sizeA.x.CompareTo(sizeB.x);
            }
        }

        private class HeightComparer : IComparer<Transform>
        {
            public int Compare(Transform a, Transform b)
            {
                Vector2 sizeA = GetTransformSize(a);
                Vector2 sizeB = GetTransformSize(b);
                return sizeA.y.CompareTo(sizeB.y);
            }
        }

        private class PositionComparerX : IComparer<Transform>
        {
            public int Compare(Transform a, Transform b) { return a.position.x.CompareTo(b.position.x); }
        }

        private class PositionComparerY : IComparer<Transform>
        {
            public int Compare(Transform a, Transform b) { return a.position.y.CompareTo(b.position.y); }
        }

        public static Transform[] SortHierarchically(Transform[] input)
        {
            Transform parent = input[0].parent;
            Transform[] result = new Transform[input.Length];
            int currentIndex = 0;

            for (int i = 0; i < parent.childCount && currentIndex < input.Length; i++)
            {
                foreach (Transform transform in input)
                {
                    if (parent.GetChild(i) != transform)
                    {
                        continue;
                    }

                    result[currentIndex] = transform;
                    currentIndex++;
                    break;
                }
            }

            return result;
        }

        public static Transform[] SortByArea(Transform[] input)
        {
            Array.Sort(input, new AreaComparer());
            return input;
        }

        public static Transform[] SortByWidth(Transform[] input)
        {
            Array.Sort(input, new WidthComparer());
            return input;
        }

        public static Transform[] SortByHeight(Transform[] input)
        {
            Array.Sort(input, new HeightComparer());
            return input;
        }

        public static Transform[] SortByPositionX(Transform[] input)
        {
            Array.Sort(input, new PositionComparerX());
            return input;
        }

        public static Transform[] SortByPositionY(Transform[] input)
        {
            Array.Sort(input, new PositionComparerY());
            return input;
        }

        public static Vector2 GetTransformSize(Transform rectTransform) { return GetTransformSize(rectTransform as RectTransform); }

        public static Vector2 GetTransformSize(RectTransform rectTransform)
        {
            Vector2 result = new Vector2();
            result.x = Mathf.Abs(rectTransform.rect.width * rectTransform.lossyScale.x);
            result.y = Mathf.Abs(rectTransform.rect.height * rectTransform.lossyScale.y);
            return result;
        }

        public static Vector2 GetLocalPivotPosition(RectTransform rectTransform)
        {
            Vector2 result = GetTransformSize(rectTransform);
            result.x *= rectTransform.pivot.x;
            result.y *= rectTransform.pivot.y;

            return result;
        }

        public static Vector2 GetPivotAndCenterLocalDistance(RectTransform rectTransform)
        {
            Vector2 size = GetTransformSize(rectTransform);

            float y = size.y * (rectTransform.pivot.y - 0.5f);
            float x = size.x * (rectTransform.pivot.x - 0.5f);

            return new Vector2(x, y);
        }

        public static SelectionStatus IsSelectionValid()
        {
            Transform[] transforms = Selection.GetTransforms(SelectionMode.Unfiltered);

            if (transforms == null || transforms.Length < 1)
            {
                return SelectionStatus.NothingSelected;
            }

            Transform sharedParent = transforms[0].parent;

            if (sharedParent == null)
            {
                return SelectionStatus.ParentIsNull;
            }

            if (sharedParent.GetComponent(typeof(RectTransform)) == null)
            {
                return SelectionStatus.ParentIsNoRectTransform;
            }

            for (int i = 1; i < transforms.Length; i++)
            {
                if (transforms[i].GetComponent(typeof(RectTransform)) == null)
                {
                    return SelectionStatus.ContainsNoRectTransform;
                }

                if (sharedParent != transforms[i].parent)
                {
                    return SelectionStatus.UnequalParents;
                }
            }

            return SelectionStatus.Valid;
        }

        public static void AdjustAnchors(RectTransform rectTransform, Vector2 oldPosition)
        {
            switch (AlignDistributeWindow.anchorMode)
            {
                case AnchorMode.StayAtCurrentPosition:
                    return;

                case AnchorMode.SnapToBorder:
                    SnapAnchorsWindow.SnapBorder(rectTransform,
                        true,
                        true,
                        true,
                        true);
                    return;

                case AnchorMode.FollowObject:
                    FollowAnchor(rectTransform, oldPosition);
                    return;

                default:
                    Debug.LogError("Unknown AnchorMode: " + AlignDistributeWindow.anchorMode);
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void FollowAnchor(RectTransform rectTransform, Vector2 oldPosition)
        {
            Vector3 currentPosition = rectTransform.position;
            Vector2 halfSize = GetTransformSize(rectTransform) * 0.5f;
            Vector2 parentSize = GetTransformSize(rectTransform.parent);
            Vector2 pivotAndCenterDistance = GetPivotAndCenterLocalDistance(rectTransform);

            Vector2 max = (Vector2) rectTransform.position + halfSize - pivotAndCenterDistance;
            Vector2 min = (Vector2) rectTransform.position - halfSize - pivotAndCenterDistance;

            Vector2 oldMax = oldPosition + halfSize - pivotAndCenterDistance;
            Vector2 oldMin = oldPosition - halfSize - pivotAndCenterDistance;

            Vector2 diffMax = oldMax - max;
            Vector2 diffMin = oldMin - min;

            // Normalize
            diffMin.x /= parentSize.x;
            diffMin.y /= parentSize.y;

            diffMax.x /= parentSize.x;
            diffMax.y /= parentSize.y;

            // Apply Values
            rectTransform.anchorMax = rectTransform.anchorMax - diffMax;
            rectTransform.anchorMin = rectTransform.anchorMin - diffMin;

            rectTransform.position = currentPosition;
        }

        public static RectTransform GetBoundingBoxRectTransform(Transform[] selection)
        {
            // Instantiating a RectTransform doesn't work, therefore we need a temporal GameObject...
            helperObject = new GameObject("Bounding Box Rect", typeof(RectTransform));
            helperObject.transform.SetParent(selection[0].parent);

            RectTransform result = helperObject.GetComponent<RectTransform>();
            Vector2 min = new Vector2(Mathf.Infinity, Mathf.Infinity);
            Vector2 max = new Vector2(Mathf.NegativeInfinity, Mathf.NegativeInfinity);

            foreach (Transform transform in selection)
            {
                RectTransform rectTransform = transform as RectTransform;

                Vector2 size = GetTransformSize(rectTransform);

                Vector2 upperRight = (Vector2) rectTransform.position + size * 0.5f - GetPivotAndCenterLocalDistance(rectTransform);
                Vector2 lowerLeft = (Vector2) rectTransform.position - size * 0.5f - GetPivotAndCenterLocalDistance(rectTransform);

                min.x = Mathf.Min(min.x, lowerLeft.x);
                min.y = Mathf.Min(min.y, lowerLeft.y);

                max.x = Mathf.Max(max.x, upperRight.x);
                max.y = Mathf.Max(max.y, upperRight.y);
            }

            result.position = new Vector3(min.x + max.x, min.y + max.y) * 0.5f;
            result.sizeDelta = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));

            return result;
        }
    }
}