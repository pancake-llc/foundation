namespace Pancake.Core
{
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class TweenExtension
    {
        public static ITween TweenColor(this Image image, Color to, float duration)
        {
            return Tween.To(() => image.color,
                current => image.color = current,
                () => to,
                duration,
                () => image != null);
        }

        public static ITween TweenColorNoAlpha(this Image image, Color to, float duration)
        {
            return Tween.To(() => image.color,
                current => image.color = current.ChangeAlpha(image.color.a),
                () => to,
                duration,
                () => image != null);
        }

        public static ITween TweenColorAlpha(this Image image, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => image.color.a,
                current => image.color = image.color.ChangeAlpha(current),
                () => to,
                duration,
                () => image != null);
        }

        public static ITween TweenFillAmount(this Image image, float to, float duration)
        {
            return Tween.To(() => image.fillAmount,
                current => image.fillAmount = current,
                () => to,
                duration,
                () => image != null);
        }
    }
}