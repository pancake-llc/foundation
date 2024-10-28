namespace Pancake.Pattern
{
    /// <summary> It allows alter dynamically behaviors. </summary>
    public abstract class Decorable
    {
        /// <summary> Change the behavior. </summary>
        /// <value>Decorator component</value>
        public IDecorator Decorator { get; set; }

        /// <summary> Perform the operation if there is an associated behavior. </summary>
        public void Operation() => Decorator?.Operation();
    }

    /// <summary> It allows alter dynamically behaviors. </summary>
    public abstract class Decorable<TR>
    {
        /// <summary> Change the behavior. </summary>
        /// <value>Decorator component</value>
        public IDecorator<TR> Decorator { get; set; }

        /// <summary> Perform the operation if there is an associated behavior. </summary>
        /// <value>Value</value>
        public TR Operation() => Decorator != null ? Decorator.Operation() : default;
    }

    /// <summary> It allows alter dynamically behaviors. </summary>
    public abstract class Decorable<TR, T>
    {
        /// <summary> Change the behavior. </summary>
        /// <value>Decorator component</value>
        public IDecorator<TR, T> Decorator { get; set; }

        /// <summary> Perform the operation if there is an associated behavior. </summary>
        /// <param name="value">Value</param>
        /// <returns>Value</returns>
        public TR Operation(T value) => Decorator != null ? Decorator.Operation(value) : default;
    }

    /// <summary> It allows alter dynamically behaviors. </summary>
    public abstract class Decorable<TR, T0, T1>
    {
        /// <summary> Change the behavior. </summary>
        /// <value>Decorator component</value>
        public IDecorator<TR, T0, T1> Decorator { get; set; }

        /// <summary> Perform the operation if there is an associated behavior. </summary>
        /// <param name="value0">First value</param>
        /// <param name="value1">Second value</param>
        /// <returns>Value</returns>
        public TR Operation(T0 value0, T1 value1) => Decorator != null ? Decorator.Operation(value0, value1) : default;
    }
}