using UnityEngine;
using UnityEngine.Events;

namespace Pancake
{
    /// <summary>
    /// BaseStackStateComponent
    /// </summary>
    public abstract class BaseStackStateComponent : BaseBehaviour, IStackState
    {
        public virtual void OnReset() { }
        public virtual void OnPush() { }
        public virtual void OnPop() { }
        public virtual void OnSuspend() { }
        public virtual void OnResume() { }
        public virtual void OnUpdate(float deltaTime) { }
    }


    /// <summary>
    /// StackStateComponent. OnEnter & OnExit can be serialized.
    /// </summary>
    [AddComponentMenu("State Machines/Stack State Component")]
    [DisallowMultipleComponent]
    public class StackStateComponent : BaseStackStateComponent
    {
        [SerializeField] private UnityEvent onPush = default;

        [SerializeField] private UnityEvent onPop = default;

        [SerializeField] private UnityEvent onSuspend = default;

        [SerializeField] private UnityEvent onResume = default;

        public event UnityAction OnPushEvent
        {
            add
            {
                if (onPush == null) onPush = new UnityEvent();
                onPush.AddListener(value);
            }
            remove => onPush?.RemoveListener(value);
        }

        public event UnityAction OnPopEvent
        {
            add
            {
                if (onPop == null) onPop = new UnityEvent();
                onPop.AddListener(value);
            }
            remove => onPop?.RemoveListener(value);
        }

        public event UnityAction OnSuspendEvent
        {
            add
            {
                if (onSuspend == null) onSuspend = new UnityEvent();
                onSuspend.AddListener(value);
            }
            remove => onSuspend?.RemoveListener(value);
        }

        public event UnityAction OnResumeEvent
        {
            add
            {
                if (onResume == null) onResume = new UnityEvent();
                onResume.AddListener(value);
            }
            remove => onResume?.RemoveListener(value);
        }

        public override void OnPush() { onPush?.Invoke(); }

        public override void OnPop() { onPop?.Invoke(); }

        public override void OnSuspend() { onSuspend?.Invoke(); }

        public override void OnResume() { onResume?.Invoke(); }
    } // class StackStateComponent
} // namespace UnityExtensions