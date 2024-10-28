namespace Pancake.Pattern
{
    /// <summary> Strategy interface. </summary>
    public interface IStrategy
    {
        /// <summary> Execute the strategy. </summary>
        void OnExecute();
    }

    /// <summary> Strategy interface. </summary>
    public interface IStrategy<out TR>
    {
        /// <summary> Execute the strategy. </summary>
        TR OnExecute();
    }

    /// <summary> Strategy interface with one parameter. </summary>
    public interface IStrategy<out TR, in T>
    {
        /// <summary> Execute the strategy. </summary>
        TR OnExecute(T value);
    }

    /// <summary> Strategy interface with two parameters. </summary>
    public interface IStrategy<out TR, in T0, in T1>
    {
        /// <summary> Execute the strategy. </summary>
        TR OnExecute(T0 value0, T1 value1);
    }

    /// <summary> Strategy interface with three parameters. </summary>
    public interface IStrategy<out TR, in T0, in T1, in T2>
    {
        /// <summary> Execute the strategy. </summary>
        TR OnExecute(T0 value0, T1 value1, T2 value2);
    }
}