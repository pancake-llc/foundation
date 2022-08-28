namespace Pancake.Core.Tween
{
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class TweenExtension
    {
        public static ITween TweenColor(this Graphic graphic, Color to, float duration)
        {
            return Tween.To(() => graphic.color,
                current => graphic.color = current,
                () => to,
                duration,
                () => graphic != null);
        }

        public static ITween TweenColorNoAlpha(this Graphic graphic, Color to, float duration)
        {
            return Tween.To(() => graphic.color,
                current => graphic.color = current.ChangeAlpha(graphic.color.a),
                () => to,
                duration,
                () => graphic != null);
        }

        public static ITween TweenColorAlpha(this Graphic graphic, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => graphic.color.a,
                current => graphic.color = graphic.color.ChangeAlpha(current),
                () => to,
                duration,
                () => graphic != null);
        }
    }
}