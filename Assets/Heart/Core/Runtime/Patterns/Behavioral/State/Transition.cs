namespace Pancake.Pattern
{
    /// <summary> Transition base class. </summary>
    public class Transition : ITransition
    {
        /// <inheritdoc/>
        public IState From { get; set; }

        /// <inheritdoc/>
        public IState To { get; set; }

        /// <inheritdoc/>
        public ICondition Condition { get; set; }

        /// <inheritdoc/>
        public bool Evaluate() => Condition != null && Condition.Check();
    }
}