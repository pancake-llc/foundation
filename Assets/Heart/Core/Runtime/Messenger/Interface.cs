namespace Pancake
{
    public interface IMessage
    {
    }

    internal interface IEventBindingInternal<T> where T : struct, IMessage
    {
        public System.Action<T> OnEvent { get; set; }
        public System.Action OnEventArgs { get; set; }
    }
}