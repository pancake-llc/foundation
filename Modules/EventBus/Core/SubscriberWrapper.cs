using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pancake.EventBus
{
    /// <summary>
    /// Subscriber container, helper class
    /// </summary>
    public sealed class SubscriberWrapper : IDisposable, ISubscriberWrapper
    {
        internal static Stack<SubscriberWrapper> s_WrappersPool = new Stack<SubscriberWrapper>(512);

        private ISubscriberName m_Name;
        private ISubscriberPriority m_Priority;

        internal bool        IsActive;
        public   Type        Key;
        public   ISubscriber Subscriber;
        public   string      Name   => m_Name.Name;
        public   ISubscriber Target => Subscriber;
        public   int         Order  => m_Priority.Priority;
        public   int         Index  { get; set; }

        // =======================================================================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke<TEvent, TInvoker>(in TEvent e, in TInvoker invoker) where TInvoker : IEventInvoker
        {
#if  DEBUG
            invoker.Invoke(in e, in Subscriber);
#else
            try
            {
                invoker.Invoke(in e, in Subscriber);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"{this}; Exception: {exception}");
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SubscriberWrapper(ISubscriber listener, Type type)
        {
            Setup(listener, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Setup(ISubscriber listener, Type type)
        {
            IsActive   = true;
            Subscriber = listener;
            m_Name     = listener as ISubscriberName ?? Extensions.s_DefaultSubscriberName;
            m_Priority = listener as ISubscriberPriority ?? Extensions.s_DefaultSubscriberPriority;
            Key        = type;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return Subscriber == ((SubscriberWrapper)obj).Subscriber;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Subscriber.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            IsActive   = false;
            Subscriber = null;
            m_Name     = null;
            m_Priority = null;
            s_WrappersPool.Push(this);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SubscriberWrapper Create(ISubscriber listener, Type key)
        {
            if (s_WrappersPool.Count > 0)
            {
                var wrapper = s_WrappersPool.Pop();
                wrapper.Setup(listener, key);
                return wrapper;
            }
            else
                return new SubscriberWrapper(listener, key);
        }

        public override string ToString()
        {
            return $"Sub: <Name> {Name}, <Type> {Subscriber.GetType()}, <Key> {Key}";
        }
    }
}