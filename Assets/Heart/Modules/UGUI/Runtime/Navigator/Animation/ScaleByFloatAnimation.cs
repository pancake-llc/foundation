#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class ScaleByFloatShowAnimation : ScaleShowAnimation
    {
        [SerializeField] private float from;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.OutQuart;

        public override async UniTask AnimateAsync(RectTransform rectTransform)
        {
            await LMotion.Create(Vector3.one * from, Vector3.one, duration)
                .WithDelay(startDelay)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.Update)
                .BindToLocalScale(rectTransform);
        }
    }

    [Serializable]
    public class ScaleByFloatHideAnimation : ScaleHideAnimation
    {
        [SerializeField] private float to;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.InQuart;

        public override async UniTask AnimateAsync(RectTransform rectTransform)
        {
            await LMotion.Create(Vector3.one, to * Vector3.one, duration)
                .WithDelay(startDelay)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.Update)
                .BindToLocalScale(rectTransform);
        }
    }
}
#endif