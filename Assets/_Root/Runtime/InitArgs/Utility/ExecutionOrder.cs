namespace Pancake.Init
{
    /// <summary>
    /// Defines the default execution orders of various components.
    /// <para>
    /// Certain event functions on scripts with lower values
    /// are executed before ones on scripts with larger values.
    /// </para>
    /// </summary>
    public static class ExecutionOrder
    {
        /// <summary>
        /// Default execution order for the <see cref="Referenceable"/> component.
        /// </summary>
        public const int Referenceable = -32000;

        /// <summary>
        /// Default execution order for the <see cref="Services"/> component.
        /// </summary>
        public const int Services = -31000;

        /// <summary>
        /// Default execution order for all <see cref="Initializer{,}">Initializer</see> components targeting a class that has the <see cref="ServiceAttribute"/>.
        /// </summary>
        public const int ServiceInitializer = -30000;

        /// <summary>
        /// Default execution order for all <see cref="Initializer{,}">Initializer</see> components targeting a class that does not have the <see cref="ServiceAttribute"/>.
        /// </summary>
        public const int Initializer = -29000;
    }
}