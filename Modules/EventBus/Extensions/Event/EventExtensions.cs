using System;

namespace Pancake.EventBus
{
    public static class EventExtensions
    {
        // to the bus
        public static void SendEvent<TKey>(this IEventBus bus, in TKey key)
        {
            bus.Send<IEvent<TKey>>(new Event<TKey>(in key));
        }

        public static void SendEvent<TKey, TData>(this IEventBus bus, in TKey key, in TData data)
        {
            bus.Send<IEvent<TKey>>(new EventData<TKey, TData>(in key, in data));
        }
        
        public static void SendEvent<TKey>(this IEventBus bus, in TKey key, params object[] data)
        {
            bus.Send<IEvent<TKey>>(new EventData<TKey, object[]>(in key, in data));
        }

        public static void SendEvent<TKey>(this IEventBus bus, in TKey key, Func<ISubscriber, bool> check, params object[] data)
        {
            bus.Send<IEvent<TKey>>(new EventData<TKey, object[]>(in key, in data), check);
        }

        public static void SendEvent<TKey>(this IEventBus bus, in Func<ISubscriber, bool> check, in TKey key)
        {
            bus.Send<IEvent<TKey>>(new Event<TKey>(in key), in check);
        }

        public static void SendEvent<TKey, TData>(this IEventBus bus, in TKey key, in Func<ISubscriber, bool> check, in TData data)
        {
            bus.Send<IEvent<TKey>>(new EventData<TKey, TData>(in key, in data), in check);
        }
        
        public static void SendEvent<TKey>(this IEventBus bus, in TKey key, in Func<ISubscriber, bool> check, params object[] data)
        {
            bus.Send<IEvent<TKey>>(new EventData<TKey, object[]>(in key, in data), in check);
        }

        // to the listener
        public static void SendEvent<TKey>(this IListener<IEvent<TKey>> receiver, in TKey key)
        {
            receiver.React(new Event<TKey>(in key));
        }

        public static void SendEvent<TKey, TData>(this IListener<IEvent<TKey>> receiver, in TKey key, in TData data)
        {
            receiver.React(new EventData<TKey, TData>(in key, in data));
        }

        public static void SendEvent<TKey>(this IListener<IEvent<TKey>> receiver, in TKey key, params object[] data)
        {
            receiver.React(new EventData<TKey, object[]>(in key, in data));
        }
        

        public static TData GetData<TData>(this IEventBase e)
        {
            // try get data
            return ((IEventData<TData>)e).Data;
        }
        
        public static bool TryGetData<TData>(this IEventBase e, out TData data)
        {
            // try get data
            if (e is IEventData<TData> eventData)
            {
                data = eventData.Data;
                return true;
            }

            data = default;
            return false;
        }

        // deconstructors
        public static (T1, T2) GetData<T1, T2>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1]);
        }

        public static (T1, T2, T3) GetData<T1, T2, T3>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2]);
        }

        public static (T1, T2, T3, T4) GetData<T1, T2, T3, T4>(this IEventBase e)
        {
            // try get data
            var dataArray = e.GetData<object[]>();

            return ((T1)dataArray[0], (T2)dataArray[1], (T3)dataArray[2], (T4)dataArray[3]);
        }

        public static bool TryGetData<T1, T2>(this IEventBase e, out T1 dataA, out T2 dataB)
        {
            // safe deconstruction version
            if (e.TryGetData(out object[] dataArray) == false || dataArray.Length < 4)
            {
                dataA = default;
                dataB = default;
                return false;
            }

            try 
            { 
                dataA = (T1)dataArray[0];
                dataB = (T2)dataArray[1];
                return true;
            }
            catch 
            {
                dataA = default;
                dataB = default;
                return false;
            }
        }

        public static bool TryGetData<T1, T2, T3>(this IEventBase e, out T1 dataA, out T2 dataB, out T3 dataC)
        {
            // safe deconstruction version
            if (e.TryGetData(out object[] dataArray) == false || dataArray.Length < 4)
            {
                dataA = default;
                dataB = default;
                dataC = default;
                return false;
            }

            try 
            { 
                dataA = (T1)dataArray[0];
                dataB = (T2)dataArray[1];
                dataC = (T3)dataArray[2];
                return true;
            }
            catch 
            {
                dataA = default;
                dataB = default;
                dataC = default;
                return false;
            }
        }

        public static bool TryGetData<T1, T2, T3, T4>(this IEventBase e, out T1 dataA, out T2 dataB, out T3 dataC, out T4 dataD)
        {
            // safe deconstruction version
            if (e.TryGetData(out object[] dataArray) == false || dataArray.Length < 4)
            {
                dataA = default;
                dataB = default;
                dataC = default;
                dataD = default;
                return false;
            }

            try 
            { 
                dataA = (T1)dataArray[0];
                dataB = (T2)dataArray[1];
                dataC = (T3)dataArray[2];
                dataD = (T4)dataArray[3];
                return true;
            }
            catch 
            {
                dataA = default;
                dataB = default;
                dataC = default;
                dataD = default;
                return false;
            }
        }
    }
    
    public sealed partial class GlobalBus
    {
        /// <summary> Send IEvent message </summary>
        public static void SendEvent<TKey>(in TKey key)
        { 
            Instance.SendEvent(in key);
        }

        /// <summary> Send IEventData message with filtration </summary>
        public static void SendEvent<TKey, TData>(in TKey key, in Func<ISubscriber, bool> check, in TData data)
        {
            Instance.SendEvent(in key, in check, in data);
        }

        /// <summary> Send IEventData message with filtration </summary>
        public static void SendEvent<TKey>(in TKey key, in Func<ISubscriber, bool> check, params object[] data)
        {
            Instance.SendEvent(in key, in check, data);
        }
        
        /// <summary> Send IEventData message </summary>
        public static void SendEvent<TKey, TData>(in TKey key, in TData data)
        {
            Instance.SendEvent(in key, in data);
        }

        /// <summary> Send IEventData message </summary>
        public static void SendEvent<TKey>(in TKey key, params object[] data)
        {
            Instance.SendEvent(in key, in data);
        }
    }
}