using Alchemy.Inspector;
using PrimeTween;
using UnityEngine;

namespace Pancake.Component
{
    [EditorIcon("csharp")]
    public class ScaleComponent : GameComponent
    {
        [SerializeField] private float delay;
        [SerializeField] private float duration;
        [SerializeField] private Vector3 value;
        [SerializeField] private bool startWith;

        [SerializeField, ShowIf(nameof(startWith)), LabelText("     Value")]
        private Vector3 startValue;

        [SerializeField] private bool loop;
        [SerializeField, ShowIf(nameof(loop)), LabelText("      Mode")] private CycleMode cycleMode = CycleMode.Restart;
        [SerializeField] private Ease ease;

        private Tween _tween;

        protected void OnEnable()
        {
            var cycles = 0;
            if (loop) cycles = -1;
            if (startWith) transform.localScale = startValue;
            _tween = Tween.Delay(delay)
            .OnComplete(() =>
            {
                _tween = Tween.Scale(transform,
                    value,
                    duration,
                    ease,
                    cycles,
                    cycleMode);
            });
        }

        protected void OnDisable() { _tween.Stop(); }
    }
}