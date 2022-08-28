namespace Pancake.Core.Tween
{
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class TweenExtension
    {
        public static ITween TweenNormalizedPosition(this ScrollRect scrollRect, Vector2 to, float duration)
        {
            return Tween.To(() => scrollRect.normalizedPosition,
                current => scrollRect.normalizedPosition = current,
                () => to,
                duration,
                () => scrollRect != null);
        }

        public static ITween TweenHorizontalNormalizedPosition(this ScrollRect scrollRect, float to, float duration)
        {
            return Tween.To(() => scrollRect.horizontalNormalizedPosition,
                current => scrollRect.horizontalNormalizedPosition = current,
                () => to,
                duration,
                () => scrollRect != null);
        }

        public static ITween TweenVerticalNormalizedPosition(this ScrollRect scrollRect, float to, float duration)
        {
            return Tween.To(() => scrollRect.verticalNormalizedPosition,
                current => scrollRect.verticalNormalizedPosition = current,
                () => to,
                duration,
                () => scrollRect != null);
        }
    }
}