namespace Pancake.Pattern
{
    /// <summary> Operation that can be altered by decorators. </summary>
    public interface IDecorator
    {
        /// <summary> The operation to be executed. </summary>
        void Operation();
    }

    /// <summary> Operation that can be altered by decorators. </summary>
    public interface IDecorator<out TR>
    {
        /// <summary> The operation to be executed. </summary>
        TR Operation();
    }

    /// <summary> Operation that can be altered by decorators. </summary>
    public interface IDecorator<out TR, in T>
    {
        /// <summary> The operation to be executed. </summary>
        TR Operation(T value);
    }

    /// <summary> Operation that can be altered by decorators. </summary>
    public interface IDecorator<out TR, in T0, in T1>
    {
        /// <summary> The operation to be executed. </summary>
        TR Operation(T0 value0, T1 value1);
    }
}