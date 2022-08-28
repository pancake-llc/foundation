namespace Pancake.Core.Tween
{
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class TweenExtension
    {
        public static ITween TweenColor(this Text text, Color to, float duration)
        {
            return Tween.To(() => text.color,
                current => text.color = current,
                () => to,
                duration,
                () => text != null);
        }

        public static ITween TweenColorNoAlpha(this Text text, Color to, float duration)
        {
            return Tween.To(() => text.color,
                current => text.color = current.ChangeAlpha(text.color.a),
                () => to,
                duration,
                () => text != null);
        }

        public static ITween TweenColorAlpha(this Text text, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => text.color.a,
                current => text.color = text.color.ChangeAlpha(current),
                () => to,
                duration,
                () => text != null);
        }
    }
}