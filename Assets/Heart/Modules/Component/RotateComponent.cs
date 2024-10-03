using UnityEngine;
using Sirenix.OdinInspector;
#if PANCAKE_LITMOTION
using LitMotion;
using LitMotion.Extensions;
#endif

namespace Pancake.Component
{
    [EditorIcon("icon_default")]
    public class RotateComponent : GameComponent
    {
        [SerializeField] private float delay;
        [SerializeField] private float duration;
        [SerializeField] private Vector3 value;
        [SerializeField] private bool startWith;

        [SerializeField, ShowIf(nameof(startWith)), LabelText("     Value")]
        private Vector3 startValue;

        [SerializeField] private bool loop;

#if PANCAKE_LITMOTION
        [SerializeField, ShowIf(nameof(loop)), LabelText("      Mode")]
        private LoopType loopType = LoopType.Restart;

        [SerializeField] private Ease ease;

        private MotionHandle _handle;
#endif

        protected void OnEnable()
        {
            var cycles = 0;
            if (loop) cycles = -1;
            if (startWith) transform.eulerAngles = startValue;
#if PANCAKE_LITMOTION
            _handle = LMotion.Create(transform.eulerAngles, value, duration)
                .WithDelay(delay)
                .WithEase(ease)
                .WithLoops(cycles, loopType)
                .BindToLocalEulerAngles(transform)
                .AddTo(gameObject);
#endif
        }

        protected void OnDisable()
        {
#if PANCAKE_LITMOTION
            if (_handle.IsActive()) _handle.Cancel();
#endif
        }
    }
}