#if PANCAKE_UNITASK
using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Pancake.UI
{
    [Serializable]
    public class RotateByVector3ShowAnimation : RotateShowAnimation
    {
        [SerializeField] private Vector3 from;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.OutQuart;

        public override async UniTask AnimateAsync(RectTransform rectTransform)
        {
            await LMotion.Create(from, Vector3.zero, duration)
                .WithDelay(startDelay)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.Update)
                .BindToLocalEulerAngles(rectTransform);
        }
    }

    [Serializable]
    public class RotateByVector3HideAnimation : RotateHideAnimation
    {
        [SerializeField] private Vector3 to;
        [SerializeField] private float startDelay;
        [SerializeField] private float duration = 0.25f;
        [SerializeField] private Ease ease = Ease.InQuart;

        public override async UniTask AnimateAsync(RectTransform rectTransform)
        {
            await LMotion.Create(Vector3.zero, to, duration)
                .WithDelay(startDelay)
                .WithEase(ease)
                .WithScheduler(MotionScheduler.Update)
                .BindToLocalEulerAngles(rectTransform);
        }
    }
}
#endif