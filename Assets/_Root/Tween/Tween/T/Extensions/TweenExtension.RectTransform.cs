namespace Pancake.Core
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenAnchorMax(this RectTransform rectTransform, Vector2 to, float duration)
        {
            return Tween.To(() => rectTransform.anchorMax,
                current => rectTransform.anchorMax = current,
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchorMaxX(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.anchorMax.x,
                current => rectTransform.anchorMax = new Vector2(current, rectTransform.anchorMax.y),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchorMaxY(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.anchorMax.y,
                current => rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, current),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchorMin(this RectTransform rectTransform, Vector2 to, float duration)
        {
            return Tween.To(() => rectTransform.anchorMin,
                current => rectTransform.anchorMin = current,
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchorMinX(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.anchorMin.x,
                current => rectTransform.anchorMin = new Vector2(current, rectTransform.anchorMin.y),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchorMinY(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.anchorMin.y,
                current => rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, current),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchoredPosition(this RectTransform rectTransform, Vector2 to, float duration)
        {
            return Tween.To(() => rectTransform.anchoredPosition,
                current => rectTransform.anchoredPosition = current,
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchoredPositionX(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.anchoredPosition.x,
                current => rectTransform.anchoredPosition = new Vector2(current, rectTransform.anchoredPosition.y),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchoredPositionY(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.anchoredPosition.y,
                current => rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, current),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenAnchoredPosition3D(this RectTransform rectTransform, Vector3 to, float duration)
        {
            return Tween.To(() => rectTransform.anchoredPosition3D,
                current => rectTransform.anchoredPosition3D = current,
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenPivot(this RectTransform rectTransform, Vector2 to, float duration)
        {
            return Tween.To(() => rectTransform.pivot,
                current => rectTransform.pivot = current,
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenSizeDelta(this RectTransform rectTransform, Vector2 to, float duration)
        {
            return Tween.To(() => rectTransform.sizeDelta,
                current => rectTransform.sizeDelta = current,
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenSizeDeltaX(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.sizeDelta.x,
                current => rectTransform.sizeDelta = new Vector2(current, rectTransform.sizeDelta.y),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenSizeDeltaY(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.sizeDelta.y,
                current => rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, current),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenOffsetMax(this RectTransform rectTransform, Vector2 to, float duration)
        {
            return Tween.To(() => rectTransform.offsetMax,
                current => rectTransform.offsetMax = current,
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenOffsetMaxX(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.offsetMax.x,
                current => rectTransform.offsetMax = new Vector2(current, rectTransform.offsetMax.y),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenOffsetMaxY(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.offsetMax.y,
                current => rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, current),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenSizeWithCurrentAnchors(this RectTransform rectTransform, Vector2 to, float duration)
        {
            return Tween.To(() => rectTransform.rect.size,
                current =>
                {
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, current.x);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, current.y);
                },
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenSizeXWithCurrentAnchors(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.rect.size.x,
                current => rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, current),
                () => to,
                duration,
                () => rectTransform != null);
        }

        public static ITween TweenSizeYWithCurrentAnchors(this RectTransform rectTransform, float to, float duration)
        {
            return Tween.To(() => rectTransform.rect.size.y,
                current => rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, current),
                () => to,
                duration,
                () => rectTransform != null);
        }
    }
}