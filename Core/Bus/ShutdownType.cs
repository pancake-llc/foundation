namespace Pancake
{
    /// <summary>
    /// Turn of bus type
    /// </summary>
    public enum ShutdownType
    {
        /// <summary>
        /// only stop the bus
        /// </summary>
        None = 0,

        /// <summary>
        /// stop bus and restart bus
        /// </summary>
        Restart,

        /// <summary>
        /// stop bus and exit game
        /// </summary>
        Quit
    }
}