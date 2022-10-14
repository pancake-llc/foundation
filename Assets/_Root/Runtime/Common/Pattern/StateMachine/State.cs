using System;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake
{
    /// <summary>
    /// BaseState
    /// </summary>
    public abstract class BaseState : IState
    {
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
        public virtual void OnUpdate(float deltaTime) { }
    }


    /// <summary>
    /// State. OnEnter & OnExit can be serialized.
    /// </summary>
    [Serializable]
    public class State : BaseState
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