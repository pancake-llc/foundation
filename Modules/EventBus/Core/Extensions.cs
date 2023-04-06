using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pancake.EventBus
{
    public static class Extensions
    {
        internal static          Dictionary<Type, Type[]>  s_SubscribersTypeCache      = new Dictionary<Type, Type[]>();
        internal static readonly DefaultSubscriberName     s_DefaultSubscriberName     = new DefaultSubscriberName();
        internal static readonly DefaultSubscriberPriority s_DefaultSubscriberPriority = new DefaultSubscriberPriority();
        public static readonly   DefaultInvoker            s_DefaultInvoker            = new DefaultInvoker();

        // =======================================================================
        public class DefaultInvoker : IEventInvoker
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Invoke<TEvent>(in TEvent e, in ISubscriber listener)
            {
                ((IListener<TEvent>)listener).React(in e);
            }
        }

        public class DefaultInvokerConditional : IEventInvoker
        {
            public Func<ISubscriber, bool>    m_Filter;

            // =======================================================================
            public void Invoke<TEvent>(in TEvent e, in ISubscriber listener)
            {
                if (m_Filter(listener))
                    ((IListener<TEvent>)listener).React(in e);
            }
        }

        // =======================================================================
        public static void Send<TEvent>(this IEventBus bus, in TEvent e)
        {
            bus.Send(in e, in s_DefaultInvoker);
        }

        public static void Send<TEvent>(this IEventBus bus, in TEvent e, in Func<ISubscriber, bool> check)
        {
            bus.Send(in e, new DefaultInvokerConditional() { m_Filter = check });
        }

        public static void Send<TEvent>(this IListener<TEvent> listener, in TEvent e)
        {
            s_DefaultInvoker.Invoke(in e, listener);
        }

        public static IEnumerable<SubscriberWrapper> ExtractWrappers(this ISubscriber listener)
        {
            var listenerType = listener.GetType();

            // try get cache
            if (s_SubscribersTypeCache.TryGetValue(listenerType, out var types))
                return types.Select(type => SubscriberWrapper.Create(listener, type));

            // extract, get type arguments from implemented ISubscriber<> interfaces
            types = listenerType.GetInterfaces()
                                .Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(ISubscriber<>))
                                .Select(n => n.GenericTypeArguments[0])
                                .ToArray();

            // add to cache
            s_SubscribersTypeCache.Add(listenerType, types);
            return types.Select(type => SubscriberWrapper.Create(listener, type));
        }
    }
}