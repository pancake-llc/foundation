namespace Pancake.Pattern
{
    /// <summary> Interface for state. </summary>
    public interface IState
    {
        /// <summary> When the status is set (for a change or for a transition). </summary>
        void OnEnter();

        /// <summary> If the status is active, each frame is executed. </summary>
        void OnUpdate();

        /// <summary> When changing to another state or terminating the state machine. </summary>
        void OnExit();
    }
}