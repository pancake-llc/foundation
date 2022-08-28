using System.Collections;
using UnityEngine;

namespace Pancake.Core
{
    public static partial class C
    {
        /// <summary>
        /// Coroutine used to make the character's sprite flicker (when hurt for example).
        /// </summary>
        public static IEnumerator Flicker(this Renderer renderer, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
        {
            if (renderer == null)
            {
                yield break;
            }

            if (!renderer.material.HasProperty("_Color"))
            {
                yield break;
            }

            if (initialColor == flickerColor)
            {
                yield break;
            }

            float flickerStop = Time.time + flickerDuration;

            while (Time.time < flickerStop)
            {
                renderer.material.color = flickerColor;
                yield return Routine.WaitFor(flickerSpeed);
                renderer.material.color = initialColor;
                yield return Routine.WaitFor(flickerSpeed);
            }

            renderer.material.color = initialColor;
        }

        /// <summary>
        /// Returns true if a renderer is visible from a camera
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
        {
            var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }
    }
}