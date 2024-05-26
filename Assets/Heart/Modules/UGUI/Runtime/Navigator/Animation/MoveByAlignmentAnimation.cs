#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class MoveByAlignmentShowAnimation : MoveShowAnimation
    {
        [SerializeField] private EAlignment from;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.Linear;

        public override async UniTask AnimateAsync(RectTransform rectTransform)
        {
            await LMotion.Create(PositionFromAlignment(rectTransform, from), PositionFromAlignment(rectTransform, EAlignment.None), duration)
                .WithDelay(startDelay)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.Update)
                .BindToAnchoredPosition(rectTransform);
        }

        private Vector2 PositionFromAlignment(RectTransform rectTransform, EAlignment alignment)
        {
            var rect = rectTransform.rect;
            return alignment switch
            {
                EAlignment.Left => Vector2.left * rect.width,
                EAlignment.Top => Vector2.up * rect.height,
                EAlignment.Right => Vector2.right * rect.width,
                EAlignment.Bottom => Vector2.down * rect.height,
                _ => Vector2.zero
            };
        }
    }

    [Serializable]
    public class MoveByAlignmentHideAnimation : MoveHideAnimation
    {
        [SerializeField] private EAlignment to;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.Linear;

        public override async UniTask AnimateAsync(RectTransform rectTransform)
        {
            await LMotion.Create(PositionFromAlignment(rectTransform, EAlignment.None), PositionFromAlignment(rectTransform, to), duration)
                .WithDelay(startDelay)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.Update)
                .BindToAnchoredPosition(rectTransform);
        }

        private Vector2 PositionFromAlignment(RectTransform rectTransform, EAlignment alignment)
        {
            var rect = rectTransform.rect;
            return alignment switch
            {
                EAlignment.Left => Vector2.left * rect.width,
                EAlignment.Top => Vector2.up * rect.height,
                EAlignment.Right => Vector2.right * rect.width,
                EAlignment.Bottom => Vector2.down * rect.height,
                _ => Vector2.zero
            };
        }
    }
}
#endif