using Pancake.Apex;
using PrimeTween;
using UnityEngine;

namespace Pancake
{
    public class MoveComponent : GameComponent
    {
        [SerializeField] private bool manual;
        [SerializeField] private float duration;
        [SerializeField] private bool useTransform;

        [SerializeField, ShowIf(nameof(useTransform)), Label("      Target")]
        private Transform target;

        [SerializeField, HideIf(nameof(useTransform)), Label("     Value")]
        private Vector3 value;

        [SerializeField] private bool loop;
        [SerializeField, ShowIf(nameof(loop)), Label("      Mode")] private CycleMode cycleMode = CycleMode.Restart;
        [SerializeField] private Ease ease;

        private Tween _tween;


#if UNITY_EDITOR
        [Button, ShowIf(nameof(ShowButtonEdit))]
        private void Edit()
        {
            UnityEditor.SessionState.SetVector3($"move_component{name}_key", transform.position);
            UnityEditor.SessionState.SetBool($"move_component{name}_key", true);
        }

        [Button, ShowIf(nameof(ShowButtonRecord))]
        private void RecordPosition()
        {
            value = transform.position;
            // ReSharper disable once Unity.InefficientPropertyAccess
            transform.position = UnityEditor.SessionState.GetVector3($"move_component{name}_key", value);
            UnityEditor.SessionState.SetBool($"move_component{name}_key", false);
        }

        private bool ShowButtonRecord()
        {
            return !useTransform && UnityEditor.SessionState.GetBool($"move_component{name}_key", false);
        }
        
        private bool ShowButtonEdit()
        {
            return !useTransform && UnityEditor.SessionState.GetBool($"move_component{name}_key", false) == false;
        }

#endif

        protected override void OnEnabled()
        {
            base.OnEnabled();
            if (manual) return;
            Move();
        }

        public void Move()
        {
            _tween.Stop();
            var cyles = 0;
            if (loop) cyles = -1;
            var pos = value;
            if (useTransform) pos = target.position;
            _tween = Tween.LocalPosition(transform,
                pos,
                duration,
                ease,
                cyles,
                cycleMode);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            _tween.Stop();
        }
    }
}