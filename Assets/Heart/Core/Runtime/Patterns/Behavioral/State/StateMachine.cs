using System;
using System.Collections.Generic;

namespace Pancake.Pattern
{
    /// <summary> State machine base class. </summary>
    public class StateMachine : IStateMachine, IDisposable
    {
        /// <inheritdoc/>
        public IState CurrentState { get; protected set; }

        /// <inheritdoc/>
        public bool Running { get; set; }

        protected readonly List<ITransition> transitions = new();

        /// <summary> Constructor. </summary>
        /// <param name="running"> Update the state machine. </param>
        public StateMachine(bool running = true) => Running = running;

        /// <summary> IDisposable. </summary>
        public void Dispose() => ChangeState(null);

        /// <summary> Destructor. </summary>
        ~StateMachine() => Dispose();

        /// <inheritdoc/>
        public void Update()
        {
            if (Running && CurrentState != null)
            {
                CurrentState.OnUpdate();

                for (int i = 0; i < transitions.Count; ++i)
                {
                    if (CurrentState == transitions[i].From && transitions[i].Evaluate())
                    {
                        ChangeState(transitions[i].To);

                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void ChangeState(IState state)
        {
            if (state != CurrentState)
            {
                CurrentState?.OnExit();

                CurrentState = state;

                CurrentState?.OnEnter();
            }
        }

        /// <inheritdoc/>
        public void AddTransition(IState from, IState to, Func<bool> condition) => AddTransition(from, to, new Condition() {Function = condition});

        /// <inheritdoc/>
        public void AddTransition(IState from, IState to, ICondition condition) => transitions.Add(new Transition() {From = from, To = to, Condition = condition,});
    }
}