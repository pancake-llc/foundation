namespace Pancake
{
    using UnityEngine;

    public static partial class C
    {
        /// <summary> Copies the RectTransform settings </summary>
        /// <param name="source"> Target RectTransform </param>
        /// <param name="from"> Source RectTransform </param>
        public static void Copy(this RectTransform source, RectTransform from)
        {
            source.localScale = from.localScale;
            source.anchorMin = from.anchorMin;
            source.anchorMax = from.anchorMax;
            source.pivot = from.pivot;
            source.sizeDelta = from.sizeDelta;
            source.anchoredPosition3D = from.anchoredPosition3D;
        }

        /// <summary> Makes the RectTransform match its parent size </summary>
        /// <param name="source"> Target RectTransform </param>
        /// <param name="resetScaleToOne"> Reset LocalScale to Vector3.one </param>
        public static void FullScreen(this RectTransform source, bool resetScaleToOne = true)
        {
            if (resetScaleToOne) source.LocalScaleToOne();

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(source, "RectTransform FullScreen");
#endif
            source.AnchorMinToZero();
            source.AnchorMaxToOne();
            source.CenterPivot();
            source.SizeDeltaToZero();
            source.AnchoredPosition3DToZero();
            source.LocalPositionToZero();
        }

        /// <summary>
        /// Self filling anchor
        /// </summary>
        /// <param name="source"></param>
        public static void SelfFilling(this RectTransform source)
        {
            RectTransform parent = null;
            if (source.parent) parent = source.parent.GetComponent<RectTransform>();
            if (!parent) return;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(source, "RectTransform SelfFilling");
#endif

            var rect = parent.rect;
            source.AnchorMinToValue(new Vector2(source.anchorMin.x + source.offsetMin.x / rect.width, source.anchorMin.y + source.offsetMin.y / rect.height));
            source.AnchorMaxToValue(new Vector2(source.anchorMax.x + source.offsetMax.x / rect.width, source.anchorMax.y + source.offsetMax.y / rect.height));
            source.OffsetMinZero();
            source.OffsetMaxZero();
            source.CenterPivot();
        }

        /// <summary> Moves the RectTransform pivot settings to its center </summary>
        /// <param name="source"> Target RectTransform </param>
        /// <param name="resetScaleToOne"> Reset LocalScale to Vector3.one </param>
        public static void Center(this RectTransform source, bool resetScaleToOne)
        {
            if (resetScaleToOne) source.LocalScaleToOne();

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(source, "RectTransform Center");
#endif

            source.AnchorMinToCenter();
            source.AnchorMaxToCenter();
            source.CenterPivot();
            source.SizeDeltaToZero();
        }

        /// <summary> Resets the target's anchoredPosition3D to Vector3.zero </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void AnchoredPosition3DToZero(this RectTransform source) { source.anchoredPosition3D = Vector3.zero; }

        /// <summary> Resets the target's localPosition to Vector3.zero </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void LocalPositionToZero(this RectTransform source) { source.localPosition = Vector3.zero; }

        /// <summary> Resets the target's localScale to Vector3.one </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void LocalScaleToOne(this RectTransform source) { source.localScale = Vector3.one; }

        /// <summary> Resets the target's anchorMin to Vector2.zero </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void AnchorMinToZero(this RectTransform source) { source.AnchorMinToValue(Vector2.zero); }

        /// <summary> Sets the target's anchorMin to Vector2(0.5f, 0.5f) </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void AnchorMinToCenter(this RectTransform source) { source.AnchorMinToValue(new Vector2(0.5f, 0.5f)); }

        /// <summary>
        /// Sets the target's anchorMin to vector2 value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        public static void AnchorMinToValue(this RectTransform source, Vector2 value) { source.anchorMin = value; }

        /// <summary> Resets the target's anchorMax to Vector2.one </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void AnchorMaxToOne(this RectTransform source) { source.AnchorMaxToValue(Vector2.one); }

        /// <summary> Sets the target's anchorMax to Vector2(0.5f, 0.5f) </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void AnchorMaxToCenter(this RectTransform source) { source.AnchorMaxToValue(new Vector2(0.5f, 0.5f)); }

        /// <summary>
        /// Sets the target's anchorMax to vector2 value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        public static void AnchorMaxToValue(this RectTransform source, Vector2 value) { source.anchorMax = value; }

        /// <summary> Sets the target's pivot to Vector2(0.5f, 0.5f) </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void CenterPivot(this RectTransform source) { source.pivot = new Vector2(0.5f, 0.5f); }

        /// <summary> Sets the target's pivot to Vector2(0f, 0.5f) </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void LeftPivot(this RectTransform source) { source.pivot = new Vector2(0f, 0.5f); }

        /// <summary> Sets the target's pivot to Vector2(1f, 0.5f) </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void RightPivot(this RectTransform source) { source.pivot = new Vector2(1f, 0.5f); }

        /// <summary> Sets the target's pivot to Vector2(0.5f, 0f) </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void TopPivot(this RectTransform source) { source.pivot = new Vector2(0.5f, 0f); }

        /// <summary> Sets the target's pivot to Vector2(0.5f, 1f) </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void BottomPivot(this RectTransform source) { source.pivot = new Vector2(0.5f, 1f); }

        /// <summary>
        /// set new value of <paramref name="pivot"/> for <paramref name="source"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pivot"></param>
        public static void SetPivot(this RectTransform source, Vector2 pivot)
        {
            if (source == null) return;
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(source, "RectTransform SetPivot");
#endif
            var size = source.rect.size;
            var deltaPivot = source.pivot - pivot;
            var deltaPosition = new Vector3(deltaPivot.x * size.x, deltaPivot.y * size.y);
            source.pivot = pivot;
            source.localPosition -= deltaPosition;
        }

        /// <summary> Resets the target's sizeDelta to Vector2.zero </summary>
        /// <param name="source"> Target RectTransform </param>
        public static void SizeDeltaToZero(this RectTransform source) { source.sizeDelta = Vector2.zero; }

        /// <summary>
        /// Set min offset to zero
        /// </summary>
        /// <param name="source"></param>
        public static void OffsetMinZero(this RectTransform source) { source.OffsetMin(Vector2.zero); }

        /// <summary>
        /// Set max offset to zero
        /// </summary>
        /// <param name="source"></param>
        public static void OffsetMaxZero(this RectTransform source) { source.OffsetMax(Vector2.zero); }

        /// <summary>
        /// Sets the left offset of a rect transform to the specified value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="left"></param>
        public static void SetLeft(this RectTransform source, float left) { source.OffsetMin(new Vector2(left, source.offsetMin.y)); }

        /// <summary>
        /// Sets the right offset of a rect transform to the specified value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="right"></param>
        public static void SetRight(this RectTransform source, float right) { source.OffsetMax(new Vector2(-right, source.offsetMax.y)); }

        /// <summary>
        /// Sets the top offset of a rect transform to the specified value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="top"></param>
        public static void SetTop(this RectTransform source, float top) { source.OffsetMax(new Vector2(source.offsetMax.x, -top)); }

        /// <summary>
        /// Sets the bottom offset of a rect transform to the specified value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bottom"></param>
        public static void SetBottom(this RectTransform source, float bottom) { source.OffsetMin(new Vector2(source.offsetMin.x, bottom)); }

        /// <summary>
        /// Set offset min to vector2 value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        public static void OffsetMin(this RectTransform source, Vector2 value) { source.offsetMin = value; }

        /// <summary>
        /// Set offset max to vector2 value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="value"></param>
        public static void OffsetMax(this RectTransform source, Vector2 value) { source.offsetMax = value; }

        /// <summary>
        /// get Corner from rectTransform
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector3[] GetCorners(this RectTransform source)
        {
            Vector3[] corners = new Vector3[4];
            source.GetWorldCorners(corners);
            return corners;
        }

        /// <summary>
        /// get Corner MaxY from rectTransform
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float MaxCornerY(this RectTransform source) { return source.GetCorners()[1].y; }

        /// <summary>
        /// get Corner MinY from rectTransform
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float MinCornerY(this RectTransform source) { return source.GetCorners()[0].y; }

        /// <summary>
        /// get Corner MaxX from rectTransform
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float MaxCornerX(this RectTransform source) { return source.GetCorners()[2].x; }

        /// <summary>
        /// get Corner MinX from rectTransform
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static float MinCornerX(this RectTransform source) { return source.GetCorners()[0].x; }

        /// <summary>
        /// Returns true if this rectangle intersects the other specified rectangle
        /// </summary>
        /// <param name="thisRectangle"></param>
        /// <param name="otherRectangle"></param>
        /// <returns></returns>
        public static bool Intersects(this Rect thisRectangle, Rect otherRectangle)
        {
            return !(thisRectangle.x > otherRectangle.xMax || thisRectangle.xMax < otherRectangle.x || thisRectangle.y > otherRectangle.yMax ||
                     thisRectangle.yMax < otherRectangle.y);
        }

        /// <summary>
        /// Returns true if the rectangle of RectTransform a overlaps rectangle of RectTransform b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Overlaps(this RectTransform a, RectTransform b) { return a.WorldRect().Overlaps(b.WorldRect()); }

        /// <summary>
        /// Returns true if the rectangle of RectTransform a overlaps rectangle of RectTransform b
        /// If allowInverse is present and true, the widths and heights of the Rects are allowed to take negative values (ie, the min value is greater than the max), and the test will still work
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="allowInverse"></param>
        /// <returns></returns>
        public static bool Overlaps(this RectTransform a, RectTransform b, bool allowInverse) { return a.WorldRect().Overlaps(b.WorldRect(), allowInverse); }

        /// <summary>
        /// Return world rect of RectTransform
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Rect WorldRect(this RectTransform source)
        {
            var sizeDelta = source.sizeDelta;
            var rectTransformWidth = sizeDelta.x * source.lossyScale.x;
            // ReSharper disable once Unity.InefficientPropertyAccess
            var rectTransformHeight = sizeDelta.y * source.lossyScale.y;

            var position = source.position;
            return new Rect(position.x - rectTransformWidth / 2f, position.y - rectTransformHeight / 2f, rectTransformWidth, rectTransformHeight);
        }
        
        public static Rect Encompass(this Rect rect, float x, float y)
        {
            if (rect.xMin > x) rect.xMin = x;
            if (rect.yMin > y) rect.yMin = y;
            if (rect.xMax < x) rect.xMax = x;
            if (rect.yMax < y) rect.yMax = y;
            return rect;
        }

        public static Rect Encompass(this Rect rect, Vector2 point) { return rect.Encompass(point.x, point.y); }

        public static Rect Encompass(this Rect? rect, Vector2 point) { return rect?.Encompass(point) ?? new Rect(point.x, point.y, 0, 0); }

        public static Rect Encompass(this Rect? rect, Rect other) { return rect?.Encompass(other) ?? other; }

        public static Rect? Encompass(this Rect? rect, Rect? other)
        {
            if (rect == null) return other;
            return other == null ? rect : rect.Value.Encompass(other.Value);
        }

        public static Rect Encompass(this Rect rect, Rect other)
        {
            // Micro-optim
            var xMin = other.xMin;
            var xMax = other.xMax;
            var yMin = other.yMin;
            var yMax = other.yMax;

            rect = rect.Encompass(xMin, yMin);
            rect = rect.Encompass(xMin, yMax);
            rect = rect.Encompass(xMax, yMin);
            rect = rect.Encompass(xMax, yMax);

            return rect;
        }

        public static bool Encompasses(this Rect rect, Rect other)
        {
            return rect.Contains(new Vector2(other.xMin, other.yMin)) && rect.Contains(new Vector2(other.xMin, other.yMax)) &&
                   rect.Contains(new Vector2(other.xMax, other.yMin)) && rect.Contains(new Vector2(other.xMax, other.yMax));
        }
        
        public static Rect ToScreenRect(this RectTransform self, bool startAtBottom = false, Canvas canvas = null, bool localTransform = false)
        {
            var corners = new Vector3[4];
            var screenCorners = new Vector3[2];

            if (localTransform)
            {
                self.GetLocalCorners(corners);
            }
            else
            {
                self.GetWorldCorners(corners);
            }

            int idx1 = startAtBottom ? 0 : 1;
            int idx2 = startAtBottom ? 2 : 3;


            if (canvas != null && (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace))
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[idx1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, corners[idx2]);
            }
            else
            {
                screenCorners[0] = RectTransformUtility.WorldToScreenPoint(null, corners[idx1]);
                screenCorners[1] = RectTransformUtility.WorldToScreenPoint(null, corners[idx2]);
            }

            if (!startAtBottom)
            {
                screenCorners[0].y = Screen.height - screenCorners[0].y;
                screenCorners[1].y = Screen.height - screenCorners[1].y;
            }

            return new Rect(screenCorners[0], screenCorners[1] - screenCorners[0]);
        }

        /// <summary>
        /// Sets the x/y transform.anchoredPosition using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetAnchoredPosition(x: 5), for example, only changing transform.anchoredPositon.x
        /// </summary>
        /// <param name="transform">The transform to set the transform.anchoredPosition at.</param>
        /// <param name="x">If this is not null, transform.anchoredPosition.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.anchoredPosition.y is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static RectTransform SetAnchoredPosition(this RectTransform transform, float? x = null, float? y = null)
        {
            transform.anchoredPosition = transform.position.Change(x, y);
            return transform;
        }
        
        /// <summary>
        /// Sets width/height using optional parameters, keeping all undefined values as they were before. Can be
        /// called with named parameters like transform.SetSizeDelta(x: 5), for example, only changing transform.width .x
        /// </summary>
        /// <param name="transform">The transform to set the transform.sizeDelta at.</param>
        /// <param name="x">If this is not null, transform.width.x is set to this value.</param>
        /// <param name="y">If this is not null, transform.height.y is set to this value.</param>
        /// <returns>The transform itself.</returns>
        public static RectTransform SetSizeDelta(this RectTransform transform, float? x = null, float? y = null)
        {
            transform.sizeDelta = transform.sizeDelta.Change(x, y);
            return transform;
        }
        
        public static RectTransform SetAnchorMin(this RectTransform transform, float? x = null, float? y = null)
        {
            transform.anchorMin = transform.anchorMin.Change(x, y);
            return transform;
        }
        
        public static RectTransform SetAnchorMax(this RectTransform transform, float? x = null, float? y = null)
        {
            transform.anchorMax = transform.anchorMax.Change(x, y);
            return transform;
        }
        
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            rect.min = rectTransform.TransformPoint(rect.min);
            rect.max = rectTransform.TransformPoint(rect.max);
            return rect;
        }
        
        /// <summary>
        /// Get the overlapped rect.
        /// </summary>
        public static Rect GetIntersection(this Rect rect, Rect other)
        {
            if (rect.xMin > other.xMin) other.xMin = rect.xMin;
            if (rect.xMax < other.xMax) other.xMax = rect.xMax;
            if (rect.yMin > other.yMin) other.yMin = rect.yMin;
            if (rect.yMax < other.yMax) other.yMax = rect.yMax;
            return other;
        }
    }
}