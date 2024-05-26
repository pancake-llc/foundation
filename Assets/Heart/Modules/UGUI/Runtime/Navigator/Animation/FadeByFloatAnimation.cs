#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class FadeByFloatShowAnimation : FadeShowAnimation
    {
        [SerializeField] private float from;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.Linear;

        public override async UniTask AnimateAsync(CanvasGroup canvasGroup)
        {
            await LMotion.Create(from, 1, duration).WithDelay(startDelay).WithEase(ease).WithScheduler(MotionScheduler.Update).BindToCanvasGroupAlpha(canvasGroup);
        }
    }

    [Serializable]
    public class FadeByFloatHideAnimation : FadeHideAnimation
    {
        [SerializeField] private float to;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.Linear;

        public override async UniTask AnimateAsync(CanvasGroup canvasGroup)
        {
            await LMotion.Create(1, to, duration).WithDelay(startDelay).WithEase(ease).WithScheduler(MotionScheduler.Update).BindToCanvasGroupAlpha(canvasGroup);
        }
    }
}
#endif