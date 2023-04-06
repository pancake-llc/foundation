namespace Pancake.EventBus
{
    // helper generic
    public abstract class Listener<A> : Subscriber, IListener<A>
    {
        public abstract void React(in A e);
    }

    public abstract class Listener<A, B> : Listener<A>, IListener<B>
    {
        public abstract void React(in B e);
    }
    
    public abstract class Listener<A, B, C> :  Listener<A, B>, IListener<C>
    {
        public abstract void React(in C e);
    }
    
    public abstract class Listener<A, B, C, D> : Listener<A, B, C>, IListener<D>
    {
        public abstract void React(in D e);
    }
}