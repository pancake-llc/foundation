using System;
using System.Collections.Generic;

namespace Pancake.EventBus
{
    public sealed class GenericListener<T> : IListener<T>, ISubscriberOptions, IDisposable
    {
        private string m_Name;
        private int    m_Order;

        public string Name     => m_Name;
        public int    Priority => m_Order;

        private List<IEventBus> m_Subscriptions = new List<IEventBus>(1);
        private Action<T>       m_Reaction;

        // =======================================================================
        public GenericListener(Action<T> reaction, int order, string name)
        {
            m_Reaction = reaction;
            m_Order    = order;
            m_Name     = name;
        }

        public void Subscribe(IEventBus bus)
        {
            if (m_Subscriptions.Contains(bus))
                return;
            m_Subscriptions.Add(bus);
            bus.Subscribe(this);
        }

        public void UnSubscribe(IEventBus bus)
        {
            if (m_Subscriptions.Remove(bus))
                bus.UnSubscribe(this);
        }

        public void Dispose()
        {
            foreach (var bus in m_Subscriptions)
            {
                bus.UnSubscribe(this);
            }
        }

        public void React(in T e)
        {
            m_Reaction(e);
        }
    }
}