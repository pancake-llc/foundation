using PrimeTween;
using UnityEngine;

namespace Pancake.Component
{
    [EditorIcon("csharp")]
    public class ScaleComponent : GameComponent
    {
        [SerializeField] private float duration;
        [SerializeField] private Vector3 value;
        [SerializeField] private bool loop;
        [SerializeField] private Ease ease;

        private Tween _tween;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            var loopCount = 0;
            if (loop) loopCount = -1;
            _tween = Tween.Scale(transform, value, duration, ease, loopCount);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            _tween.Stop();
        }
    }
}