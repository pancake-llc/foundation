using System;
using System.Collections.Generic;

namespace Pancake.EventBus
{
    /// <summary> Event messages receiver interface </summary>
    public interface IEventBus : ISubscriber
    {
        void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) 
            where TInvoker : IEventInvoker;
        
        void Subscribe(ISubscriber sub);
        void UnSubscribe(ISubscriber sub);
    }

    /// <summary> Implementation </summary>
    public interface IEventBusImpl : IDisposable
    {
        void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
            where TInvoker : IEventInvoker;

        void Subscribe(ISubscriber sub);
        void UnSubscribe(ISubscriber sub);

        void Subscribe(SubscriberWrapper sub);

        IEnumerable<ISubscriberWrapper> GetSubscribers();
    }

    /// <summary> Invokes events on the listener </summary>
    public interface IEventInvoker
    {
        void Invoke<TEvent>(in TEvent e, in ISubscriber listener);
    }

    /// <summary> Container interface without generic constraints </summary>
    public interface ISubscriber
    {
    }
    
    /// <summary> Marker interface, basically event key container in form of generic argument </summary>
    public interface ISubscriber<TEvent>
    {
    }

    /// <summary> Reaction interface </summary>
    public interface IListener<TEvent> : ISubscriber<TEvent>, ISubscriber
    {
        void React(in TEvent e);
    }

    /// <summary> Provides additional options for event listener </summary>
    public interface ISubscriberOptions : ISubscriberPriority, ISubscriberName
    {
    }
    
    public interface ISubscriberPriority
    {
        /// <summary> Order in listeners queue, lower first, same order listeners will be added at the back of the ordered stack </summary>
        int         Priority { get; }
    }
    
    public interface ISubscriberName
    {
        /// <summary> Listener id, used in logs </summary>
        string      Name { get; }
    }

    public interface ISubscriberWrapper
    {
        int         Order  { get; }
        int         Index  { get; }
        ISubscriber Target { get; }

        void Invoke<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker;
    }
}