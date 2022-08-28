namespace Pancake.Core
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenTime(this TrailRenderer trailRenderer, float to, float duration)
        {
            return Tween.To(() => trailRenderer.time,
                current => trailRenderer.time = current,
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenStartColor(this TrailRenderer trailRenderer, Color to, float duration)
        {
            return Tween.To(() => trailRenderer.startColor,
                current => trailRenderer.startColor = current,
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenEndColor(this TrailRenderer trailRenderer, Color to, float duration)
        {
            return Tween.To(() => trailRenderer.endColor,
                current => trailRenderer.endColor = current,
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenColor(this TrailRenderer trailRenderer, Color to, float duration)
        {
            ITween startTween = TweenStartColor(trailRenderer, to, duration);
            ITween endTween = TweenEndColor(trailRenderer, to, duration);

            GroupTween groupTween = new GroupTween();
            groupTween.Add(startTween);
            groupTween.Add(endTween);

            return groupTween;
        }

        public static ITween TweenStartColorNoAlpha(this TrailRenderer trailRenderer, Color to, float duration)
        {
            return Tween.To(() => trailRenderer.startColor,
                current => trailRenderer.startColor = current.ChangeAlpha(trailRenderer.startColor.a),
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenEndColorNoAlpha(this TrailRenderer trailRenderer, Color to, float duration)
        {
            return Tween.To(() => trailRenderer.endColor,
                current => trailRenderer.endColor = current.ChangeAlpha(trailRenderer.endColor.a),
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenColorNoAlpha(this TrailRenderer trailRenderer, Color to, float duration)
        {
            ITween startTween = TweenStartColorNoAlpha(trailRenderer, to, duration);
            ITween endTween = TweenEndColorNoAlpha(trailRenderer, to, duration);

            GroupTween groupTween = new GroupTween();
            groupTween.Add(startTween);
            groupTween.Add(endTween);

            return groupTween;
        }

        public static ITween TweenStartColorAlpha(this TrailRenderer trailRenderer, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => trailRenderer.startColor.a,
                current => trailRenderer.startColor = trailRenderer.startColor.ChangeAlpha(current),
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenEndColorAlpha(this TrailRenderer trailRenderer, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => trailRenderer.endColor.a,
                current => trailRenderer.endColor = trailRenderer.endColor.ChangeAlpha(current),
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenColorAlpha(this TrailRenderer trailRenderer, float to, float duration)
        {
            ITween startTween = TweenStartColorAlpha(trailRenderer, to, duration);
            ITween endTween = TweenEndColorAlpha(trailRenderer, to, duration);

            GroupTween groupTween = new GroupTween();
            groupTween.Add(startTween);
            groupTween.Add(endTween);

            return groupTween;
        }

        public static ITween TweenStartWidth(this TrailRenderer trailRenderer, float to, float duration)
        {
            return Tween.To(() => trailRenderer.startWidth,
                current => trailRenderer.startWidth = current,
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenEndWidth(this TrailRenderer trailRenderer, float to, float duration)
        {
            return Tween.To(() => trailRenderer.endWidth,
                current => trailRenderer.endWidth = current,
                () => to,
                duration,
                () => trailRenderer != null);
        }

        public static ITween TweenWidth(this TrailRenderer trailRenderer, float to, float duration)
        {
            ITween startTween = TweenStartWidth(trailRenderer, to, duration);
            ITween endTween = TweenEndWidth(trailRenderer, to, duration);

            GroupTween groupTween = new GroupTween();
            groupTween.Add(startTween);
            groupTween.Add(endTween);

            return groupTween;
        }
    }
}