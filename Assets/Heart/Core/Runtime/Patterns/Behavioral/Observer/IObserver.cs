namespace Pancake.Pattern
{
    /// <summary> Observer Interface. </summary>
    public interface IObserver
    {
        /// <summary> When the Observer is notified  </summary>
        void OnNotify();
    }

    /// <summary> Observer Interface, one parameter. </summary>
    public interface IObserver<in T>
    {
        /// <summary> When the Observer is notified  </summary>
        /// <param name="value">Parameter</param>
        void OnNotify(T value);
    }

    /// <summary> Observer Interface, two parameters. </summary>
    public interface IObserver<in T0, in T1>
    {
        /// <summary> When the Observer is notified  </summary>
        /// <param name="value0">Parameter</param>
        /// <param name="value1">Parameter</param>
        void OnNotify(T0 value0, T1 value1);
    }

    /// <summary> Observer Interface, three parameters. </summary>
    public interface IObserver<in T0, in T1, in T2>
    {
        /// <summary> When the Observer is notified  </summary>
        /// <param name="value0">Parameter</param>
        /// <param name="value1">Parameter</param>
        /// <param name="value2">Parameter</param>
        void OnNotify(T0 value0, T1 value1, T2 value2);
    }
}