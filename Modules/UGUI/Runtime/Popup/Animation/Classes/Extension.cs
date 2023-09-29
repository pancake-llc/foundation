using System;
using System.Collections;
using UnityEngine;

namespace Pancake.UI
{
    internal static class TransitionExtension
    {
        public static IEnumerator CreateRoutine(this ITransitionAnimation transition, IProgress<float> progress = null)
        {
            var player = new AnimationPlayer(transition);

            App.Add(UpdateMode.Update, Update);
            progress?.Report(0.0f);
            player.Play();
            while (!player.IsFinished)
            {
                yield return null;
                progress?.Report(player.Time / transition.Duration);
            }

            App.Remove(UpdateMode.Update, Update);
            yield break;

            void Update() => player.Update(Time.unscaledDeltaTime);
        }
    }

    internal static class AlgimentExtension
    {
        public static Vector3 ToPosition(this Alignment alignment, RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height;
            float z = rectTransform.localPosition.z;
            var position = alignment switch
            {
                Alignment.Left => new Vector3(-width, 0, z),
                Alignment.Right => new Vector3(width, 0, z),
                Alignment.Top => new Vector3(0, height, z),
                Alignment.Bottom => new Vector3(0, -height, z),
                Alignment.Center => new Vector3(0, 0, z),
                _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
            };

            return position;
        }
    }
}