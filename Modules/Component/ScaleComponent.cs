using Pancake.Tween;
using UnityEngine;

namespace Pancake.Component
{
    public class ScaleComponent : GameComponent
    {
        [SerializeField] private float duration;
        [SerializeField] private Vector3 value;
        [SerializeField] private bool loop;
        [SerializeField] private Ease ease;

        private Tween.Tween _tween;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            var loopCount = 0;
            if (loop) loopCount = -1;
            _tween = Tween.Tween.Create(false).SetLoop(loopCount, true).Add(transform.ActionScale(value, duration).SetEase(ease)).Play();
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            _tween?.Stop();
        }
    }
}