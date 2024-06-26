using System;
using System.Collections;
using Pancake.Common;
using UnityEngine;

namespace Pancake.UI
{
    internal static class TransitionExtension
    {
        public static IEnumerator CreateRoutine(this ITransitionAnimation transition, IProgress<float> progress = null)
        {
            var player = new AnimationPlayer(transition);

            App.AddListener(EUpdateMode.Update, Update);
            progress?.Report(0.0f);
            player.Play();
            while (!player.IsFinished)
            {
                yield return null;
                progress?.Report(player.Time / transition.Duration);
            }

            App.RemoveListener(EUpdateMode.Update, Update);
            yield break;

            void Update() => player.Update(Time.unscaledDeltaTime);
        }
    }

    internal static class AlgimentExtension
    {
        public static Vector3 ToPosition(this EAlignment alignment, RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            float width = rect.width;
            float height = rect.height;
            float z = rectTransform.localPosition.z;
            var position = alignment switch
            {
                EAlignment.Left => new Vector3(-width, 0, z),
                EAlignment.Right => new Vector3(width, 0, z),
                EAlignment.Top => new Vector3(0, height, z),
                EAlignment.Bottom => new Vector3(0, -height, z),
                EAlignment.Center => new Vector3(0, 0, z),
                _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null)
            };

            return position;
        }
    }
}