using System;
#if PANCAKE_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace Pancake.UI
{
    internal static class TransitionExtension
    {
#if PANCAKE_UNITASK
        public static async UniTask PlayWith(this ITransitionAnimation transition, IProgress<float> progress = null)
        {
            var player = new AnimationPlayer(transition);

            progress?.Report(0.0f);
            player.Play();

            while (!player.IsFinished)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);
                player.Update(Time.unscaledDeltaTime);
                progress?.Report(player.Time / transition.Duration);
            }

            player.Stop();
            progress?.Report(1.0f);
        }
#endif
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