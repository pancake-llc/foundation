using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Core
{
    public static partial class C
    {
        /// <summary>
        /// Fade out canvas group
        /// </summary>
        /// <param name="canvasGroup"></param>
        /// <param name="endValue"></param>
        /// <param name="speed"></param>
        /// <param name="delay"></param>
        public static void FadeOut(this CanvasGroup canvasGroup, float endValue, float speed, float delay = 0)
        {
            if (delay == 0)
            {
                IEFadeOutImpl(canvasGroup, endValue, speed).RunCoroutine();
            }
            else
            {
                IEFadeOutImpl(canvasGroup, endValue, speed).Delay(delay).RunCoroutine();
            }
        }

        private static IEnumerator<float> IEFadeOutImpl(CanvasGroup canvasGroup, float endValue, float speed)
        {
            while (canvasGroup.alpha > endValue)
            {
                canvasGroup.alpha -= Time.unscaledDeltaTime * speed;
                yield return Timing.WaitForOneFrame;
            }

            canvasGroup.alpha = endValue;
        }

        /// <summary>
        /// Fade in canvas group
        /// </summary>
        /// <param name="canvasGroup"></param>
        /// <param name="endValue"></param>
        /// <param name="speed"></param>
        /// <param name="delay"></param>
        public static void FadeIn(this CanvasGroup canvasGroup, float endValue, float speed, float delay = 0)
        {
            if (delay == 0)
            {
                FadeInImpl(canvasGroup, endValue, speed).RunCoroutine();
            }
            else
            {
                FadeInImpl(canvasGroup, endValue, speed).Delay(delay).RunCoroutine();
            }
        }

        private static IEnumerator<float> FadeInImpl(CanvasGroup canvasGroup, float endValue, float speed)
        {
            while (canvasGroup.alpha < endValue)
            {
                canvasGroup.alpha += Time.unscaledDeltaTime * speed;
                yield return Timing.WaitForOneFrame;
            }

            canvasGroup.alpha = endValue;
        }
    }
}