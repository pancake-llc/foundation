namespace Pancake.Elm
{
    public interface IMessenger<T> where T : struct
    {
        T GetMessage();
    }
}