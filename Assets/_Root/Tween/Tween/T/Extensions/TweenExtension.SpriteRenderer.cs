namespace Pancake.Core
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenColor(this SpriteRenderer spriteRenderer, Color to, float duration)
        {
            return Tween.To(() => spriteRenderer.color,
                current => spriteRenderer.color = current,
                () => to,
                duration,
                () => spriteRenderer != null);
        }

        public static ITween TweenColorNoAlpha(this SpriteRenderer spriteRenderer, Color to, float duration)
        {
            return Tween.To(() => spriteRenderer.color,
                current => spriteRenderer.color = current.ChangeAlpha(spriteRenderer.color.a),
                () => to,
                duration,
                () => spriteRenderer != null);
        }

        public static ITween TweenColorAlpha(this SpriteRenderer spriteRenderer, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => spriteRenderer.color.a,
                current => spriteRenderer.color = spriteRenderer.color.ChangeAlpha(current),
                () => to,
                duration,
                () => spriteRenderer != null);
        }
    }
}