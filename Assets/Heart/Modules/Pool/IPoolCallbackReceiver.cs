namespace Pancake.Pools
{
    public interface IPoolCallbackReceiver
    {
        void OnRequest();
        void OnReturn();
    }
}