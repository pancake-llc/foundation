using UnityEngine;
using UnityEngine.Events;

namespace Pancake
{
    /// <summary>
    /// BaseStateComponent
    /// </summary>
    public abstract class BaseStateComponent : BaseBehaviour, IState
    {
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate(float deltaTime) { }
    }


    /// <summary>
    /// StateComponent. OnEnter & OnExit can be serialized.
    /// </summary>
    [AddComponentMenu("State Machines/State Component")]
    [DisallowMultipleComponent]
    public class StateComponent : BaseStateComponent
    {
        [SerializeField] private UnityEvent onEnter = default;

        [SerializeField] private UnityEvent onExit = default;

        public event UnityAction OnEnterEvent
        {
            add
            {
                if (onEnter == null) onEnter = new UnityEvent();
                onEnter.AddListener(value);
            }
            remove => onEnter?.RemoveListener(value);
        }

        public event UnityAction OnExitEvent
        {
            add
            {
                if (onExit == null) onExit = new UnityEvent();
                onExit.AddListener(value);
            }
            remove => onExit?.RemoveListener(value);
        }


        public override void OnEnter() { onEnter?.Invoke(); }


        public override void OnExit() { onExit?.Invoke(); }
    }
}