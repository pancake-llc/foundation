using System;
using System.Collections.Generic;
using Pancake;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// StackStateMachineComponent
    /// </summary>
    public class StackStateMachineComponent<T> : BaseStackStateComponent where T : class, IStackState
    {
        private List<T> _states = new List<T>(4);
        private double _currentStateTime;


        /// <summary>
        /// Number of states in the stack.
        /// </summary>
        public int StateCount => _states.Count;


        /// <summary>
        /// Total time since entering the current state.
        /// </summary>
        public float CurrentStateTime => (float) _currentStateTime;


        /// <summary>
        /// Total time since entering the current state.
        /// </summary>
        public double CurrentStateTimeDouble => _currentStateTime;


        /// <summary>
        /// Current state (unusable if stateCount is zero)
        /// </summary>
        public T CurrentState => _states[_states.Count - 1];


        /// <summary>
        /// The state under current state (unusable if stateCount is zero or one)
        /// </summary>
        public T UnderState => _states[_states.Count - 2];


        /// <summary>
        /// Get the state in the stack by specified index.
        /// </summary>
        public T GetState(int index) { return _states[index]; }


#if DEBUG
        private bool _duringSetting = false;
#endif


        /// <summary>
        /// Push a state to the stack (state can be null).
        /// </summary>
        public void PushState(T newState)
        {
#if DEBUG
            if (_duringSetting)
            {
                throw new Exception("Can not change state inside OnExit or OnEnter!");
            }

            _duringSetting = true;
#endif

            if (StateCount > 0) CurrentState?.OnSuspend();

            _currentStateTime = 0;
            _states.Add(newState);

            newState?.OnPush();

#if DEBUG
            _duringSetting = false;
#endif

            StatePushed(newState);
        }


        /// <summary>
        /// Pop current state from the stack (unusable if stateCount is zero).
        /// </summary>
        public void PopState()
        {
#if DEBUG
            if (_duringSetting)
            {
                throw new Exception("Can not change state inside OnExit or OnEnter!");
            }

            _duringSetting = true;
#endif

            T originalState = CurrentState;

            originalState?.OnPop();

            _states.RemoveAt(_states.Count - 1);
            _currentStateTime = 0;

            if (StateCount > 0) CurrentState?.OnResume();

#if DEBUG
            _duringSetting = false;
#endif

            StatePopped(originalState);
        }


        /// <summary>
        /// Pop multi-states from the stack.
        /// </summary>
        public void PopStates(int count)
        {
            while (count > 0)
            {
                PopState();
                count--;
            }
        }


        /// <summary>
        /// Pop all states from the stack.
        /// </summary>
        public void PopAllStates() { PopStates(_states.Count); }


        /// <summary>
        /// Reset the stack.
        /// </summary>
        public void ResetStack()
        {
            while (_states.Count > 0)
            {
                int index = _states.Count - 1;
                _states[index]?.OnReset();
                _states.RemoveAt(index);
            }

            _currentStateTime = 0;
        }


        protected virtual void StatePopped(T poppedState) { }


        protected virtual void StatePushed(T pushedState) { }


        /// <summary>
        /// Update current state.
        /// Note: top level state machine need call this.
        /// </summary>
        public override void OnUpdate(float deltaTime)
        {
            _currentStateTime += deltaTime;
            if (StateCount > 0) CurrentState?.OnUpdate(deltaTime);
        }


        public override void OnReset() { ResetStack(); }
    } // class StackStateMachineComponent<T>


    [AddComponentMenu("State Machines/Stack State Machine Component")]
    [DisallowMultipleComponent]
    public class StackStateMachineComponent : StackStateMachineComponent<BaseStackStateComponent>
    {
        public TimeMode timeMode = TimeMode.Unscaled;


        protected override void Tick() { OnUpdate(timeMode == TimeMode.Normal ? Time.deltaTime : Time.unscaledDeltaTime); }
    }
}