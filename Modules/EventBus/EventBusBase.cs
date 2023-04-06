using UnityEngine;

namespace Pancake.EventBus
{
    /// <summary>
    /// EventBusImpl MonoBehaviour wrapper
    /// </summary>
    public class EventBusBase : MonoBehaviour, IEventBus
    {
        private IEventBusImpl m_Impl = new EventBusImpl();

        // =======================================================================
        protected virtual void Awake()
        {
            foreach (var sub in this.ExtractWrappers())
                m_Impl.Subscribe(sub);
        }

        public void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        {
            m_Impl.Send(in e, in invoker);
        }

        public void Subscribe(ISubscriber sub)
        {
            // add listeners to the bus
            m_Impl.Subscribe(sub);
        }

        public void UnSubscribe(ISubscriber sub)
        {
            m_Impl.UnSubscribe(sub);
        }

        public void Subscribe(IEventBus bus)
        {
            m_Impl.Subscribe(bus);
        }

        public void UnSubscribe(IEventBus bus)
        {
            m_Impl.UnSubscribe(bus);
        }

        protected virtual void OnDestroy()
        {
            m_Impl.Dispose();
            m_Impl = null;
        }
        
        // =======================================================================
        [ContextMenu("Log subscribers", false, 0)]
        public void LogSubscribers()
        {
            foreach (var subscriber in m_Impl.GetSubscribers())
                Debug.Log(subscriber.ToString(), subscriber.Target as MonoBehaviour);
        }
    }
}