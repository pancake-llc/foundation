using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.EventBus
{
    /// <summary>
    /// Event bus, with auto subscription functionality
    /// </summary>
    public class EventBus : EventBusBase, ISubscriberOptions
    {
        public string Name     => name;
        public int    Priority
        {
            get => m_Priority;
            set
            {
                if (m_Priority == value)
                    return;

                m_Priority = value;

                // reconnect if order was changed
                if (m_Connected)
                {
                    _disconnectBus();
                    _connectBus();
                }
            }
        }

        [SerializeField]
        private SubscriptionTarget m_SubscribeTo = SubscriptionTarget.Global;
        [SerializeField]
        private int                m_Priority;
        private bool               m_Connected;
        private List<IEventBus>    m_Subscriptions = new List<IEventBus>();

        // =======================================================================
        [Serializable] [Flags]
        public enum SubscriptionTarget
        {
            None = 0,
            /// <summary> EventBus singleton </summary>
            Global = 1,
            /// <summary> First parent EventBus </summary>
            FirstParent = 1 << 1,
        }

        public SubscriptionTarget SubscribeTo
        {
            get => m_SubscribeTo;
            set
            {
                if (m_SubscribeTo == value)
                    return;

                m_SubscribeTo = value;

                if (m_Connected)
                {
                    _disconnectBus();
                    _buildSubscriptionList();
                    _connectBus();
                }
                else
                    _buildSubscriptionList();
            }
        }

        // =======================================================================
        protected override void Awake()
        {
            base.Awake();
            _buildSubscriptionList();
        }

        protected virtual void OnEnable()
        {
            _connectBus();
        }

        protected virtual void OnDisable()
        {
            _disconnectBus();
        }

        // =======================================================================
        private void _disconnectBus()
        {
            if (m_Connected == false)
                return;

            m_Connected = false;
            
            foreach (var bus in m_Subscriptions)
                bus.UnSubscribe(this);
        }

        private void _connectBus()
        {
            if (m_Connected)
                return;

            m_Connected = true;
            
            foreach (var bus in m_Subscriptions)
                bus.Subscribe(this);
        }

        private void _buildSubscriptionList()
        {
            m_Subscriptions.Clear();

            if (m_SubscribeTo == SubscriptionTarget.None)
                return;

            if (m_SubscribeTo.HasFlag(SubscriptionTarget.Global) && GlobalBus.Instance != null)
                m_Subscriptions.Add(GlobalBus.Instance);

            if (m_SubscribeTo.HasFlag(SubscriptionTarget.FirstParent) && transform.parent != null)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Subscriptions.Add(firstParent);
            }
        }
    }
}