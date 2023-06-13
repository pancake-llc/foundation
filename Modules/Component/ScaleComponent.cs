using Pancake.Tween;
using UnityEngine;

namespace Pancake.Component
{
    public class ScaleComponent : GameComponent
    {
        [SerializeField] private float duration;
        [SerializeField] private Vector3 value;
        [SerializeField] private Ease ease;
        [SerializeField] private bool loop;

        private Tween.Tween _tween;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            _tween = Tween.Tween.Create(false).SetLoop(-1, true).Add(transform.ActionScale(value, duration).SetEase(ease)).Play();
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            _tween?.Stop();
        }
    }
}