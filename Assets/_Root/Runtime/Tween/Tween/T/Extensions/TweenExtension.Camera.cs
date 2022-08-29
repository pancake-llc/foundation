namespace Pancake.Tween
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenAspect(this Camera camera, float to, float duration)
        {
            return Tween.To(() => camera.aspect,
                current => camera.aspect = current,
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenBackgroundColor(this Camera camera, Color to, float duration)
        {
            return Tween.To(() => camera.backgroundColor,
                current => camera.backgroundColor = current,
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenBackgroundColorNoAlpha(this Camera camera, Color to, float duration)
        {
            return Tween.To(() => camera.backgroundColor,
                current => camera.backgroundColor = current.ChangeAlpha(camera.backgroundColor.a),
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenBackgroundColorAlpha(this Camera camera, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => camera.backgroundColor.a,
                current => camera.backgroundColor = camera.backgroundColor.ChangeAlpha(current),
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenFarClipPlane(this Camera camera, float to, float duration)
        {
            return Tween.To(() => camera.farClipPlane,
                current => camera.farClipPlane = current,
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenNearClipPlane(this Camera camera, float to, float duration)
        {
            return Tween.To(() => camera.nearClipPlane,
                current => camera.nearClipPlane = current,
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenFieldOfView(this Camera camera, float to, float duration)
        {
            return Tween.To(() => camera.fieldOfView,
                current => camera.fieldOfView = current,
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenOrthoSize(this Camera camera, float to, float duration)
        {
            return Tween.To(() => camera.orthographicSize,
                current => camera.orthographicSize = current,
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenRect(this Camera camera, Rect to, float duration)
        {
            return Tween.To(() => camera.rect,
                current => camera.rect = current,
                () => to,
                duration,
                () => camera != null);
        }

        public static ITween TweenPixelRect(this Camera camera, Rect to, float duration)
        {
            return Tween.To(() => camera.pixelRect,
                current => camera.pixelRect = current,
                () => to,
                duration,
                () => camera != null);
        }
    }
}