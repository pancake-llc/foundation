using System.Collections.Generic;
using UnityEngine;

namespace Pancake.EventBus.Utils
{
    public sealed class Logger : MonoBehaviour, IEventBus, ISubscriberOptions
    {
        public string Name     => nameof(Logger);
        public int    Priority => int.MinValue;

        [SerializeField]
        private GameObject   m_ListenTo;

        [SerializeField]
        private int          m_LogLenght = 8;
        [SerializeField]
        private List<string> m_Log;

        private IEventBus   m_ConnectedTo;

        // =======================================================================
        private void OnEnable()
        {
            if (m_ListenTo != null && m_ListenTo.TryGetComponent(out IEventBus bus))
            {
                m_ConnectedTo = bus;
                m_ConnectedTo.Subscribe(this);
            }
        }

        private void OnDisable()
        {
            if (m_ListenTo != null && m_ConnectedTo != null)
            {
                m_ConnectedTo.UnSubscribe(this);
                m_ConnectedTo = null;
            }
        }

        public void Send<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        {
            m_Log.Add(e.ToString());

            if (m_Log.Count > m_LogLenght)
                m_Log.RemoveAt(0);
        }

        public void Subscribe(ISubscriber sub)
        {
            throw new System.NotImplementedException();
        }

        public void UnSubscribe(ISubscriber sub)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(IEventBus bus)
        {
            throw new System.NotImplementedException();
        }

        public void UnSubscribe(IEventBus bus)
        {
            throw new System.NotImplementedException();
        }
    }
}