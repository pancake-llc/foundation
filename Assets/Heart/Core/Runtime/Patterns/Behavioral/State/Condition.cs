namespace Pancake.Pattern
{
    /// <summary> Condition base class. </summary>
    public class Condition : ICondition
    {
        /// <summary> Function to evaluate condition. </summary>
        public System.Func<bool> Function { get; set; }

        /// <inheritdoc/>
        public bool Check() => Function != null && Function.Invoke();
    }
}