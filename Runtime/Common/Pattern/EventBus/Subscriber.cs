using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.EventBus
{
    /// <summary> Base class for EventListener & Listener MonoBehavior </summary>
    public abstract class Subscriber : MonoBehaviour, ISubscriber, ISubscriberOptions
    {
        [SerializeField] [Tooltip("Subscription targets")]
        private SubscriptionTarget m_SubscribeTo = SubscriptionTarget.FirstParent;
        [SerializeField] [Tooltip("Listener priority, lowest first, same last")]
        private int                m_Priority;
        private bool               m_Connected;
        private List<IEventBus>    m_Buses = new List<IEventBus>();

        public List<IEventBus> Subscriptions => m_Buses;
        public string          Name          => gameObject.name;

        public int Priority
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
                    _disconnectListener();
                    _connectListener();
                }
            }
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
                    _disconnectListener();
                    _buildSubscriptionList();
                    _connectListener();
                }
                else
                    _buildSubscriptionList();
            }
        }

        // =======================================================================
        [Serializable] [Flags]
        public enum SubscriptionTarget
        {
            None = 0,
            /// <summary> EventBus singleton </summary>
            Global = 1,
            /// <summary> First parent EventBus </summary>
            FirstParent = 1 << 1,
            /// <summary> This gameObject EventBus </summary>
            This = 1 << 2,
        }

        // =======================================================================
        protected virtual void Awake()
        {
            _buildSubscriptionList();
            m_Connected = false;
        }

        protected virtual void OnEnable()
        {
            _connectListener();
        }

        protected virtual void OnDisable()
        {
            _disconnectListener();
        }

        // =======================================================================
        private void _connectListener()
        {
            // connect if disconnected
            if (m_Connected)
                return;

            foreach (var bus in m_Buses)
                bus.Subscribe(this);

            m_Connected = true;
        }

        private void _disconnectListener()
        {
            // disconnect if connected
            if (m_Connected == false)
                return;

            foreach (var bus in m_Buses)
                bus.UnSubscribe(this);

            m_Connected = false;
        }

        private void _buildSubscriptionList()
        {
            m_Buses.Clear();
            if (m_SubscribeTo == SubscriptionTarget.None)
                return;

            // EventSystem singleton
            if (m_SubscribeTo.HasFlag(SubscriptionTarget.Global) && ReferenceEquals(GlobalBus.Instance, null) == false)
                m_Buses.Add(GlobalBus.Instance);

            // first parent EventBus
            if (m_SubscribeTo.HasFlag(SubscriptionTarget.FirstParent) && ReferenceEquals(transform.parent, null) == false)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Buses.Add(firstParent);
            }

            // self if has IEventBus component
            if (m_SubscribeTo.HasFlag(SubscriptionTarget.This))
            {
                if (transform.TryGetComponent<IEventBus>(out var thisBus))
                    m_Buses.Add(thisBus);
            }
        }

        private void _resubscribe(SubscriptionTarget subscribeTo)
        {
            var unsibscribe = m_SubscribeTo & ~subscribeTo;
            var subscribe = m_SubscribeTo ^ subscribeTo;

            // unsubscribe from
            if (unsibscribe.HasFlag(SubscriptionTarget.Global))
                m_Buses.Remove(GlobalBus.Instance);
            if (unsibscribe.HasFlag(SubscriptionTarget.FirstParent))
                m_Buses.Remove(transform.parent.GetComponentInParent<IEventBus>());
            if (unsibscribe.HasFlag(SubscriptionTarget.This))
                m_Buses.Remove(GetComponent<IEventBus>());

            // subscribe to
            if (subscribe.HasFlag(SubscriptionTarget.Global) && ReferenceEquals(GlobalBus.Instance, null) == false)
                m_Buses.Remove(GlobalBus.Instance);
            if (subscribe.HasFlag(SubscriptionTarget.FirstParent) && ReferenceEquals(transform.parent, null) == false)
            {
                var firstParent = transform.parent.GetComponentInParent<IEventBus>();
                if (firstParent != null)
                    m_Buses.Add(firstParent);
            }
            if (subscribe.HasFlag(SubscriptionTarget.This))
            {
                if (transform.TryGetComponent<IEventBus>(out var thisBus))
                    m_Buses.Add(thisBus);
            }

            m_SubscribeTo = subscribeTo;
        }
    }
}