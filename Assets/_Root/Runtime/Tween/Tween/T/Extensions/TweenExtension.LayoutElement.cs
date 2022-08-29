namespace Pancake.Tween
{
    using UnityEngine;
    using UnityEngine.UI;

    public static partial class TweenExtension
    {
        public static ITween TweenFlexibleWidth(this LayoutElement layoutElement, float to, float duration)
        {
            return Tween.To(() => layoutElement.flexibleWidth,
                current => layoutElement.flexibleWidth = current,
                () => to,
                duration,
                () => layoutElement != null);
        }

        public static ITween TweenFlexibleHeight(this LayoutElement layoutElement, float to, float duration)
        {
            return Tween.To(() => layoutElement.flexibleHeight,
                current => layoutElement.flexibleHeight = current,
                () => to,
                duration,
                () => layoutElement != null);
        }

        public static ITween TweenFlexibleSize(this LayoutElement layoutElement, Vector2 to, float duration)
        {
            ITween widthTween = TweenFlexibleWidth(layoutElement, to.x, duration);
            ITween heightTween = TweenFlexibleHeight(layoutElement, to.y, duration);

            GroupTween groupTween = new GroupTween();
            groupTween.Add(widthTween);
            groupTween.Add(heightTween);

            return groupTween;
        }

        public static ITween TweenMinWidth(this LayoutElement layoutElement, float to, float duration)
        {
            return Tween.To(() => layoutElement.minWidth,
                current => layoutElement.minWidth = current,
                () => to,
                duration,
                () => layoutElement != null);
        }

        public static ITween TweenMinHeight(this LayoutElement layoutElement, float to, float duration)
        {
            return Tween.To(() => layoutElement.minHeight,
                current => layoutElement.minHeight = current,
                () => to,
                duration,
                () => layoutElement != null);
        }

        public static ITween TweenMinSize(this LayoutElement layoutElement, Vector2 to, float duration)
        {
            ITween widthTween = TweenMinWidth(layoutElement, to.x, duration);
            ITween heightTween = TweenMinHeight(layoutElement, to.y, duration);

            GroupTween groupTween = new GroupTween();
            groupTween.Add(widthTween);
            groupTween.Add(heightTween);

            return groupTween;
        }

        public static ITween TweenPreferedWidth(this LayoutElement layoutElement, float to, float duration)
        {
            return Tween.To(() => layoutElement.preferredWidth,
                current => layoutElement.preferredWidth = current,
                () => to,
                duration,
                () => layoutElement != null);
        }

        public static ITween TweenPreferedHeight(this LayoutElement layoutElement, float to, float duration)
        {
            return Tween.To(() => layoutElement.preferredHeight,
                current => layoutElement.preferredHeight = current,
                () => to,
                duration,
                () => layoutElement != null);
        }

        public static ITween TweenPreferedSize(this LayoutElement layoutElement, Vector2 to, float duration)
        {
            ITween widthTween = TweenPreferedWidth(layoutElement, to.x, duration);
            ITween heightTween = TweenPreferedHeight(layoutElement, to.y, duration);

            GroupTween groupTween = new GroupTween();
            groupTween.Add(widthTween);
            groupTween.Add(heightTween);

            return groupTween;
        }
    }
}