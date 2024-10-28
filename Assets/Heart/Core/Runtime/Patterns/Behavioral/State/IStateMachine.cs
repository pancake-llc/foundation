namespace Pancake.Pattern
{
    /// <summary> Interface for state machine. </summary>
    public interface IStateMachine
    {
        /// <summary> Current status (can be null). </summary>
        IState CurrentState { get; }

        /// <summary> Activates / deactivates the status machine. </summary>
        bool Running { get; set; }

        /// <summary> Must be executed in each frame. </summary>
        void Update();

        /// <summary> Changes the current state to other or null. </summary>
        /// <param name="state"> Other state or null. </param>
        void ChangeState(IState state);

        /// <summary> Adds a new transition between states. </summary>
        /// <param name="from"> Origin state. </param>
        /// <param name="to"> Destination state. </param>
        /// <param name="condition"> Function to make the transition. </param>
        void AddTransition(IState from, IState to, System.Func<bool> condition);

        /// <summary> Adds a new transition between states. </summary>
        /// <param name="from"> Origin state. </param>
        /// <param name="to"> Destination state. </param>
        /// <param name="condition"> Condition to make the transition. </param>
        void AddTransition(IState from, IState to, ICondition condition);
    }
}