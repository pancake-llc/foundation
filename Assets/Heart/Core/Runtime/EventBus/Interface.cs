namespace Pancake
{
    public interface IEvent
    {
    }

    internal interface IEventBindingInternal<T> where T : struct, IEvent
    {
        public System.Action<T> OnEvent { get; set; }
        public System.Action OnEventArgs { get; set; }
    }
}