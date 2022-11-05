namespace Pancake.Tween
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenColor(this Light light, Color to, float duration)
        {
            return Tween.To(() => light.color,
                current => light.color = current,
                () => to,
                duration,
                () => light != null);
        }

        public static ITween TweenIntensity(this Light light, float to, float duration)
        {
            return Tween.To(() => light.intensity,
                current => light.intensity = current,
                () => to,
                duration,
                () => light != null);
        }

        public static ITween TweenShadowStrenght(this Light light, float to, float duration)
        {
            return Tween.To(() => light.shadowStrength,
                current => light.shadowStrength = current,
                () => to,
                duration,
                () => light != null);
        }
    }
}