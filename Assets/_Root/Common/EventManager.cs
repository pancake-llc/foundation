//#define EVENTROUTER_THROWEXCEPTIONS 

using System;
using System.Collections.Generic;
using UnityEngine;

#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // Uncomment this if you want listeners to be required for sending events.
#endif

// ReSharper disable InvalidXmlDocComment
namespace Pancake.Core
{
    /// <summary>
    /// GameEvent are used throughout the game for general game events (game started, game ended, life lost, etc.)
    /// </summary>
    public struct GameEvent
    {
        private static GameEvent @event;
        public string eventName;

        public GameEvent(string eventName) { this.eventName = eventName; }

        public static void Trigger(string eventName)
        {
            @event.eventName = eventName;
            EventManager.TriggerEvent(@event);
        }
    }

    /// <summary>
    /// Static class that allows any class to start or stop listening to events
    /// </summary>
    public static class EventRegister
    {
        public delegate void Delegate<in T>(T eventType);

        public static void EventStartListening<TEventType>(this IGameEventListener<TEventType> caller) where TEventType : struct
        {
            EventManager.AddListener<TEventType>(caller);
        }

        public static void EventStopListening<TEventType>(this IGameEventListener<TEventType> caller) where TEventType : struct
        {
            EventManager.RemoveListener<TEventType>(caller);
        }
    }

    /// <summary>
    /// Event listener basic interface
    /// </summary>
    public interface IEventListener
    {
    };

    /// <summary>
    /// A public interface you'll need to implement for each type of event you want to listen to.
    /// </summary>
    public interface IGameEventListener<in T> : IEventListener
    {
        void OnEvent(T eventType);
    }

    /// <summary>
    /// This class handles event management, and can be used to broadcast events throughout the game, to tell one class (or many) that something's happened.
    /// Events are structs, you can define any kind of events you want. This manager comes with GameEvents, which are 
    /// basically just made of a string, but you can work with more complex ones if you want.
    /// 
    /// To trigger a new event, from anywhere, do YOUR_EVENT.Trigger(YOUR_PARAMETERS)
    /// So GameEvent.Trigger("Save"); for example will trigger a Save GameEvent
    /// 
    /// you can also call EventManager.TriggerEvent(YOUR_EVENT);
    /// For example : EventManager.TriggerEvent(new GameEvent("GameStart")); will broadcast an GameEvent named GameStart to all listeners.
    ///
    /// To start listening to an event from any class, there are 3 things you must do : 
    ///
    /// 1 - tell that your class implements the IEventListener interface for that kind of event.
    /// For example: public class GUIManager : Singleton<GUIManager>, IEventListener<GameEvent>
    /// You can have more than one of these (one per event type).
    ///
    /// 2 - On Enable and Disable, respectively start and stop listening to the event :
    /// void OnEnable()
    /// {
    /// 	this.EventStartListening<GameEvent>();
    /// }
    /// void OnDisable()
    /// {
    /// 	this.EventStopListening<GameEvent>();
    /// }
    /// 
    /// 3 - Implement the IEventListener interface for that event. For example :
    /// public void OnEvent(GameEvent gameEvent)
    /// {
    /// 	if (gameEvent.EventName == "GameOver")
    ///		{
    ///			// DO SOMETHING
    ///		}
    /// } 
    /// will catch all events of type GameEvent emitted from anywhere in the game, and do something if it's named GameOver
    /// </summary>
    [ExecuteAlways]
    public static class EventManager
    {
        private static readonly Dictionary<Type, List<IEventListener>> SubscribersList;

        static EventManager() { SubscribersList = new Dictionary<Type, List<IEventListener>>(); }

        /// <summary>
        /// Adds a new subscriber to a certain event.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        public static void AddListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : struct
        {
            Type eventType = typeof(TEvent);

            if (!SubscribersList.ContainsKey(eventType))
                SubscribersList[eventType] = new List<IEventListener>();

            if (!SubscriptionExists(eventType, listener))
                SubscribersList[eventType].Add(listener);
        }

        /// <summary>
        /// Removes a subscriber from a certain event.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="TEvent">The event type.</typeparam>
        public static void RemoveListener<TEvent>(IGameEventListener<TEvent> listener) where TEvent : struct
        {
            Type eventType = typeof(TEvent);

            if (!SubscribersList.ContainsKey(eventType))
            {
#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
#else
                return;
#endif
            }

            List<IEventListener> subscriberList = SubscribersList[eventType];

#if EVENTROUTER_THROWEXCEPTIONS
	            bool listenerFound = false;
#endif

            for (int i = 0; i < subscriberList.Count; i++)
            {
                if (subscriberList[i] == listener)
                {
                    subscriberList.Remove(subscriberList[i]);
#if EVENTROUTER_THROWEXCEPTIONS
					    listenerFound = true;
#endif

                    if (subscriberList.Count == 0)
                    {
                        SubscribersList.Remove(eventType);
                    }

                    return;
                }
            }

#if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
#endif
        }

        /// <summary>
        /// Triggers an event. All instances that are subscribed to it will receive it (and will potentially act on it).
        /// </summary>
        /// <param name="newEvent">The event to trigger.</param>
        /// <typeparam name="TEvent">The 1st type parameter.</typeparam>
        public static void TriggerEvent<TEvent>(TEvent newEvent) where TEvent : struct
        {
            if (!SubscribersList.TryGetValue(typeof(TEvent), out var list))
#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
#else
                return;
#endif

            for (int i = 0; i < list.Count; i++)
            {
                (list[i] as IGameEventListener<TEvent>)?.OnEvent(newEvent);
            }
        }

        /// <summary>
        /// Checks if there are subscribers for a certain type of events
        /// </summary>
        /// <returns><c>true</c>, if exists was subscriptioned, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="receiver">Receiver.</param>
        private static bool SubscriptionExists(Type type, IEventListener receiver)
        {
            if (!SubscribersList.TryGetValue(type, out var receivers)) return false;

            bool exists = false;

            for (int i = 0; i < receivers.Count; i++)
            {
                if (receivers[i] == receiver)
                {
                    exists = true;
                    break;
                }
            }

            return exists;
        }
    }
}