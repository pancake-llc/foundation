namespace Pancake.Core
{
    using UnityEngine;

    public static partial class TweenExtension
    {
        public static ITween TweenVolume(this AudioSource audioSource, float to, float duration)
        {
            return Tween.To(() => audioSource.volume,
                current => audioSource.volume = current,
                () => to,
                duration,
                () => audioSource != null);
        }

        public static ITween TweenPitch(this AudioSource audioSource, float to, float duration)
        {
            return Tween.To(() => audioSource.pitch,
                current => audioSource.pitch = current,
                () => to,
                duration,
                () => audioSource != null);
        }
    }
}