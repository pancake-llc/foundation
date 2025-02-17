namespace Pancake.Elm
{
    public delegate void Dispatcher<T>(IMessenger<T> msg) where T : struct;
}