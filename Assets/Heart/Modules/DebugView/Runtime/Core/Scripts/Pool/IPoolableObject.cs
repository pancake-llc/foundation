namespace Pancake.DebugView
{
    internal interface IPoolableObject
    {
        void OnBeforeUse();
        void OnBeforeRelease();
        void OnBeforeClear();
    }
}