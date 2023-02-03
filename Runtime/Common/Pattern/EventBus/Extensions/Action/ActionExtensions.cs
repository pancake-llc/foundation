using System;
using System.Runtime.CompilerServices;

namespace Pancake.EventBus
{
    public static class ActionExtensions
    {
        public static readonly ActionInvoker s_ActionInvoker = new ActionInvoker();

        // =======================================================================
        public class ActionInvoker: IEventInvoker
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Invoke<TEvent>(in TEvent e, in ISubscriber listener)
            {
                ((IHandleInvoker)e).Invoke(listener);
            }
        }

        public class ActionInvokerConditional: IEventInvoker
        {
            public Func<ISubscriber, bool>    m_Filter;

            // =======================================================================
            public void Invoke<TEvent>(in TEvent e, in ISubscriber listener)
            {
                if (m_Filter(listener))
                    ((IHandleInvoker)e).Invoke(listener);
            }
        }

        // =======================================================================
        public static void SendAction<THandle>(this IEventBus bus, in Action<THandle> action)
        {
            bus.Send<IHandleInvoker<THandle>, ActionInvoker>(new HandleInvoker<THandle>(action), s_ActionInvoker);
        }

        public static void SendAction<THandle>(this IEventBus bus, in Action<THandle> action, in Func<ISubscriber, bool> check)
        {
            bus.Send<IHandleInvoker<THandle>, ActionInvokerConditional>(new HandleInvoker<THandle>(action), new ActionInvokerConditional(){ m_Filter = check });
        }

        /*public static void SendAction<THandle>(this IHandle<THandle> handle, in Action<THandle> action)
        {
            s_ActionInvoker.Invoke(new HandleInvoker<THandle>(action), handle);
        }*/
    }
    
    public sealed partial class GlobalBus
    {
        public static void SendAction<THandle>(in Action<THandle> action)
        { 
            Instance.SendAction(in action);
        }

        public static void SendAction<THandle>(in Action<THandle> action, in Func<ISubscriber, bool> check)
        { 
            Instance.SendAction(in action, in check);
        }
    }
}