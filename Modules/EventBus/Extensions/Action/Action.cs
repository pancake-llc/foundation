using System;
using System.Runtime.CompilerServices;

namespace Pancake.EventBus
{
    public interface IHandle
    {
    }
    
    /// <summary> SendAction target interface </summary>
    public interface IHandle<THandle> : ISubscriber<IHandleInvoker<THandle>>, ISubscriber, IHandle
    {
    }

    /// <summary> Invocation interface without generic constraints </summary>
    public interface IHandleInvoker
    {
        Type Type { get; } 
        void Invoke(ISubscriber listener);
    }

    /// <summary> Event key interface </summary>
    public interface IHandleInvoker<THandle> : IHandleInvoker
    {
    }

    internal class HandleInvoker<THandle> : IHandleInvoker<THandle>
    {
        private Action<THandle> m_Action;
        public Type Type => typeof(THandle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke(ISubscriber listener)
        {
            m_Action.Invoke((THandle)listener);
        }

        public HandleInvoker(Action<THandle> action)
        {
            m_Action = action;
        }

        public override string ToString()
        {
            return $"Action({typeof(THandle)})";
        }

    }
}