using Pancake.Apex;
using Pancake.Tween;
using UnityEngine;

namespace Pancake
{
    public class MoveComponent : GameComponent
    {
        [SerializeField] private float duration;
        [SerializeField] private bool useTransform;

        [SerializeField, ShowIf(nameof(useTransform)), Label("      Target")]
        private Transform target;

        [SerializeField, ShowIf("ShowValue"), Label("     Value")] private Vector3 value;

        [SerializeField] private bool loop;
        [SerializeField] private Ease ease;

        private Tween.Tween _tween;


#if UNITY_EDITOR
        private bool ShowValue() => !useTransform;

        [Button, ShowIf("ShowValue")]
        private void RecordCurrentPositon() { value = transform.position; }

#endif

        protected override void OnEnabled()
        {
            base.OnEnabled();
            var loopCount = 0;
            if (loop) loopCount = -1;
            var pos = value;
            if (useTransform) pos = target.position;
            _tween = Tween.Tween.Create(false).SetLoop(loopCount, true).Add(transform.ActionMove(pos, duration).SetEase(ease)).Play();
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            _tween?.Stop();
        }
    }
}