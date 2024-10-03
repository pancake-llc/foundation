using Sirenix.OdinInspector;
#if PANCAKE_LITMOTION
using LitMotion;
using LitMotion.Extensions;
#endif
using UnityEngine;

namespace Pancake
{
    [EditorIcon("icon_default")]
    public class MoveComponent : GameComponent
    {
        [SerializeField] private bool manual;
        [SerializeField] private float duration;
        [SerializeField] private bool useTransform;

        [SerializeField, ShowIf(nameof(useTransform)), LabelText("      Target")]
        private Transform target;

        [SerializeField, HideIf(nameof(useTransform)), LabelText("     Value")]
        private Vector3 value;

        [SerializeField] private bool loop;

#if PANCAKE_LITMOTION
        [SerializeField, ShowIf(nameof(loop)), LabelText("      Mode")]
        private LoopType loopType = LoopType.Restart;

        [SerializeField] private Ease ease;
        private MotionHandle _handle;
#endif


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

        private bool ShowButtonRecord() { return !useTransform && UnityEditor.SessionState.GetBool($"move_component{name}_key", false); }

        private bool ShowButtonEdit() { return !useTransform && UnityEditor.SessionState.GetBool($"move_component{name}_key", false) == false; }

#endif

        protected void OnEnable()
        {
            if (manual) return;
            Move();
        }

        public void Move()
        {
#if PANCAKE_LITMOTION
            if (_handle.IsActive()) _handle.Cancel();
#endif
            var cyles = 0;
            if (loop) cyles = -1;
            var pos = value;
            if (useTransform) pos = target.position;
#if PANCAKE_LITMOTION
            _handle = LMotion.Create(transform.position, pos, duration).WithEase(ease).WithLoops(cyles, loopType).BindToLocalPosition(transform).AddTo(gameObject);
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