namespace Pancake.EventBus
{
    // helper generic
    public abstract class EventListener<A> : Subscriber, IListener<IEvent<A>>
    {
        public abstract void React(in IEvent<A> e);
    }

    public abstract class EventListener<A, B> : EventListener<A>, IListener<IEvent<B>>
    {
        public abstract void React(in IEvent<B> e);
    }

    public abstract class EventListener<A, B, C> : EventListener<A, B>, IListener<IEvent<C>>
    {
        public abstract void React(in IEvent<C> e);
    }
    
    public abstract class EventListener<A, B, C, D> : EventListener<A, B, C>, IListener<IEvent<D>>
    {
        public abstract void React(in IEvent<D> e);
    }

}