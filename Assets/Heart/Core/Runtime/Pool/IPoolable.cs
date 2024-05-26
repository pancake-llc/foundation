namespace Pancake
{
    public interface IPoolable
    {
        void OnRequest();

        void OnReturn();
    }
}