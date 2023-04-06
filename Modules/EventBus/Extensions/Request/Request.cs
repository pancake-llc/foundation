namespace Pancake.EventBus
{
    /// <summary> Base request class, can be only approved or ignored </summary>
    public interface IRequestBase
    {
        bool IsApproved { get; }

        void Approve();
    }
    
    /// <summary> Worker interface of request </summary>
    public interface IRequest<out TKey>: IRequestBase, IEvent<TKey>
    {
    }
    
    /// <summary> Request extends IEvent </summary>
    internal class EventRequest<TKey> : Event<TKey>, IRequest<TKey>
    {
        public  bool   IsApproved { get; private set; }

        // =======================================================================
        public EventRequest(in TKey key) : base(in key) { }

        public void Approve()
        {
            IsApproved = true;
        }
    }

    /// <summary> Request extends IEventData </summary>
    internal class EventDataRequest<TKey, TData> : EventData<TKey, TData>, IRequest<TKey>
    {
        public  bool   IsApproved { get; private set; }

        // =======================================================================
        public EventDataRequest(in TKey key, in TData data) : base(in key, in data) { }

        public void Approve()
        {
            IsApproved = true;
        }
    }
}