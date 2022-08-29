namespace Pancake.Tween
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenColor(this Material material, Color to, float duration)
        {
            return Tween.To(() => material.color,
                current => material.color = current,
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColor(this Material material, Color to, string property, float duration)
        {
            return Tween.To(() => material.GetColor(property),
                current => material.SetColor(property, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColor(this Material material, Color to, int propertyID, float duration)
        {
            return Tween.To(() => material.GetColor(propertyID),
                current => material.SetColor(propertyID, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColorNoAlpha(this Material material, Color to, float duration)
        {
            return Tween.To(() => material.color,
                current => material.color = current.ChangeAlpha(material.color.a),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColorNoAlpha(this Material material, Color to, string property, float duration)
        {
            return Tween.To(() => material.GetColor(property),
                current => material.SetColor(property, current.ChangeAlpha(material.GetColor(property).a)),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColorNoAlpha(this Material material, Color to, int propertyID, float duration)
        {
            return Tween.To(() => material.GetColor(propertyID),
                current => material.SetColor(propertyID, current.ChangeAlpha(material.GetColor(propertyID).a)),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColorAlpha(this Material material, float to, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => material.color.a,
                current => material.color = material.color.ChangeAlpha(current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColorAlpha(this Material material, float to, string property, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => material.GetColor(property).a,
                current => material.SetColor(property, material.GetColor(property).ChangeAlpha(current)),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenColorAlpha(this Material material, float to, int propertyID, float duration)
        {
            float to255 = to * 255.0f;

            return Tween.To(() => material.GetColor(propertyID).a,
                current => material.SetColor(propertyID, material.GetColor(propertyID).ChangeAlpha(current)),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenFloat(this Material material, float to, string property, float duration)
        {
            return Tween.To(() => material.GetFloat(property),
                current => material.SetFloat(property, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenFloat(this Material material, float to, int propertyID, float duration)
        {
            return Tween.To(() => material.GetFloat(propertyID),
                current => material.SetFloat(propertyID, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenVector(this Material material, Vector4 to, string property, float duration)
        {
            return Tween.To(() => material.GetVector(property),
                current => material.SetVector(property, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenVector(this Material material, Vector4 to, int propertyID, float duration)
        {
            return Tween.To(() => material.GetVector(propertyID),
                current => material.SetVector(propertyID, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenTextureOffset(this Material material, Vector2 to, float duration)
        {
            return Tween.To(() => material.mainTextureOffset,
                current => material.mainTextureOffset = current,
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenTextureOffset(this Material material, Vector2 to, string property, float duration)
        {
            return Tween.To(() => material.GetTextureOffset(property),
                current => material.SetTextureOffset(property, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenTextureOffset(this Material material, Vector2 to, int propertyID, float duration)
        {
            return Tween.To(() => material.GetTextureOffset(propertyID),
                current => material.SetTextureOffset(propertyID, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenTextureScale(this Material material, Vector2 to, float duration)
        {
            return Tween.To(() => material.mainTextureScale,
                current => material.mainTextureScale = current,
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenTextureScale(this Material material, Vector2 to, string property, float duration)
        {
            return Tween.To(() => material.GetTextureScale(property),
                current => material.SetTextureScale(property, current),
                () => to,
                duration,
                () => material != null);
        }

        public static ITween TweenTextureScale(this Material material, Vector2 to, int propertyID, float duration)
        {
            return Tween.To(() => material.GetTextureScale(propertyID),
                current => material.SetTextureScale(propertyID, current),
                () => to,
                duration,
                () => material != null);
        }
    }
}