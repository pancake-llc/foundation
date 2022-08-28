namespace Pancake.Core
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenAlpha(this CanvasGroup canvasGroup, float to, float duration)
        {
            return Tween.To(() => canvasGroup.alpha,
                current => canvasGroup.alpha = current,
                () => to,
                duration,
                () => canvasGroup != null);
        }
    }
}