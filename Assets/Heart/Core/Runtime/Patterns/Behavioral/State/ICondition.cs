namespace Pancake.Pattern
{
    /// <summary> Condition to evaluate in a transition. If true, the transition between states is performed. </summary>
    public interface ICondition
    {
        /// <summary> Evaluates the condition and returns true or false. </summary>
        /// <returns> True or false. </returns>
        bool Check();
    }
}