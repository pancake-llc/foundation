namespace Pancake.ControllerTree
{
    public enum EControllerState
    {
        None = 0,
        Initialized = 100,
        Running = 200,
        Stopping = 300,
        Stopped = 400,
        Faulted = 500,
        Disposed = 600
    }
}