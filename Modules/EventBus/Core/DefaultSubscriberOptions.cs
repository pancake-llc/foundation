namespace Pancake.EventBus
{
    internal class DefaultSubscriberName : ISubscriberName
    {
        public string Name     => string.Empty;
    }
    
    internal class DefaultSubscriberPriority : ISubscriberPriority
    {
        public int    Priority => 0;
    }
}