using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Pancake.EventBus.Utils;


namespace Pancake.EventBus
{
    /// <summary>
    /// EventBus functionality
    /// </summary>
    public class EventBusImpl : IEventBusImpl
    {
        private const int k_DefaultSetSize = 4;

        private static readonly IComparer<ISubscriberWrapper> k_OrderComparer = new SubscriberWrapperComparer();

        private Dictionary<Type, SortedCollection<SubscriberWrapper>> m_Subscribers = new Dictionary<Type, SortedCollection<SubscriberWrapper>>();
        private SortedCollection<SubscriberBusWrapper>                m_Buses       = new SortedCollection<SubscriberBusWrapper>(k_OrderComparer);
        private int                                                   m_AddIndex;

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker)
            where TInvoker : IEventInvoker
        {
            var hasListeners = m_Subscribers.TryGetValue(typeof(TEvent), out var listeners) && listeners.Count > 0;
            var hasBusses    = m_Buses.Count > 0;
            
            // optimization mess
            if (hasListeners && hasBusses)
            {
                var buses = m_Buses.m_Collection.ToArray();
                var subs = listeners.m_Collection.ToArray();

                var busIndex = 0;
                var subIndex = 0;

                var bus = buses[0];
                var sub = subs[0];

                while (true)
                {
                    // skip inactive subs, because the order check might refer to the destroyed object 
                    if (sub.IsActive == false)
                    {
                        if (++ subIndex >= subs.Length)
                        {
                            if (bus.IsActive)
                                bus.Invoke(in e, in invoker);
                            while (++ busIndex < buses.Length)
                            {
                                bus = buses[busIndex];
                                if (bus.IsActive)
                                    bus.Invoke(in e, in invoker);
                            }
                            break;
                        }

                        sub = subs[subIndex];
                        continue;
                    }

                    if (bus.IsActive == false)
                    {
                        if (++ busIndex >= buses.Length)
                        {
                            if (sub.IsActive)
                                sub.Invoke(in e, in invoker);
                            while (++ subIndex < subs.Length)
                            {
                                sub = subs[subIndex];
                                if (sub.IsActive)
                                    sub.Invoke(in e, in invoker);
                            }
                            break;
                        }

                        bus = buses[busIndex];
                        continue;
                    }

                    if (sub.Order == bus.Order ? sub.Index < bus.Index : sub.Order < bus.Order)
                    {
                        // invoke listener, move next, if no more listeners invoke remaining buses
                        sub.Invoke(in e, in invoker);
                        if (++ subIndex >= subs.Length)
                        {
                            if (bus.IsActive)
                                bus.Invoke(in e, in invoker);
                            while (++ busIndex < buses.Length)
                            {
                                bus = buses[busIndex];
                                if (bus.IsActive)
                                    bus.Invoke(in e, in invoker);
                            }
                            break;
                        }

                        sub = subs[subIndex];
                    }
                    else
                    {
                        // invoke bus, move next, if no more buses invoke remaining listeners
                        bus.Invoke(in e, in invoker);
                        if (++ busIndex >= buses.Length)
                        {
                            if (sub.IsActive)
                                sub.Invoke(in e, in invoker);
                            while (++ subIndex < subs.Length)
                            {
                                sub = subs[subIndex];
                                if (sub.IsActive)
                                    sub.Invoke(in e, in invoker);
                            }
                            break;
                        }

                        bus = buses[busIndex];
                    }
                }
            }
            else if (hasBusses)
                foreach (var bus in m_Buses.m_Collection.ToArray())
                {
                    if (bus.IsActive)
                        bus.Invoke(in e, in invoker);
                }
            else if (hasListeners)
                foreach (var listener in listeners.m_Collection.ToArray())
                {
                    if (listener.IsActive)
                        listener.Invoke(in e, in invoker);
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(ISubscriber sub)
        {
            if (sub is IEventBus bus) 
                Subscribe(bus);
            else
                foreach (var wrapper in sub.ExtractWrappers())
                    Subscribe(wrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(ISubscriber sub)
        {
            if (sub is IEventBus bus) 
                UnSubscribe(bus);
            else
                foreach (var wrapper in sub.ExtractWrappers())
                    UnSubscribe(wrapper);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(SubscriberWrapper sub)
        {
            if (sub == null)
                throw new ArgumentNullException(nameof(sub));

            sub.Index = m_AddIndex ++;

            // get or create group
            if (m_Subscribers.TryGetValue(sub.Key, out var set) == false)
            {
                set = new SortedCollection<SubscriberWrapper>(k_OrderComparer, k_DefaultSetSize);
                m_Subscribers.Add(sub.Key, set);
            }

            set.Add(sub);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(SubscriberWrapper subscriber)
        {
            if (subscriber == null)
                throw new ArgumentNullException(nameof(subscriber));

            // remove first match
            if (m_Subscribers.TryGetValue(subscriber.Key, out var set))
            {
                // remove & dispose
                if (set.Extract(in subscriber, out var extracted))
                    extracted.Dispose();

                if (set.Count == 0)
                    m_Subscribers.Remove(subscriber.Key);
            }

            subscriber.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Subscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            m_Buses.Add(SubscriberBusWrapper.Create(in bus, m_AddIndex ++));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnSubscribe(IEventBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            var busWrapper = SubscriberBusWrapper.Create(in bus, m_AddIndex ++);
            if (m_Buses.Extract(busWrapper, out var extracted))
                extracted.Dispose();

            busWrapper.Dispose();
        }

        public IEnumerable<ISubscriberWrapper> GetSubscribers()
        {
            return m_Subscribers.SelectMany<KeyValuePair<Type, SortedCollection<SubscriberWrapper>>, object>(group => group.Value).Concat(m_Buses).OfType<ISubscriberWrapper>();
        }

        public void Dispose()
        {
            foreach (var wrappers in m_Subscribers.Values)
            foreach (var wrapper in wrappers)
                wrapper.Dispose();
        }
    }
}