namespace Pancake.Core
{
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class TweenExtension
    {
        public static ITween TweenColor(this Outline outline, Color to, float duration)
        {
            return Tween.To(() => outline.effectColor,
                current => outline.effectColor = current,
                () => to,
                duration,
                () => outline != null);
        }

        public static ITween TweenColorNoAlpha(this Outline outline, Color to, float duration)
        {
            return Tween.To(() => outline.effectColor,
                current => outline.effectColor = current.ChangeAlpha(outline.effectColor.a),
                () => to,
                duration,
                () => outline != null);
        }

        public static ITween TweenColorAlpha(this Outline outline, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => outline.effectColor.a,
                current => outline.effectColor = outline.effectColor.ChangeAlpha(current),
                () => to,
                duration,
                () => outline != null);
        }
    }
}