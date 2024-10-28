namespace Pancake.Pattern
{
    /// <summary> Transition between two states. </summary>
    public interface ITransition
    {
        /// <summary> Origin state. </summary>
        IState From { get; set; }

        /// <summary> Destination state. </summary>
        IState To { get; set; }

        /// <summary> Condition that must be fulfilled to make the transition between states. </summary>
        ICondition Condition { get; set; }

        /// <summary> Evaluate the condition to make the transition or not. </summary>
        /// <returns> If the condition is met, true, otherwise false. </returns>
        bool Evaluate();
    }
}