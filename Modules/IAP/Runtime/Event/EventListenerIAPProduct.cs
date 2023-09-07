#if PANCAKE_IAP
using System;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;


namespace Pancake.IAP
{
    [EditorIcon("scriptable_event_listener")]
    [AddComponentMenu("Scriptable/EventListeners/EventListenerIAPProduct")]
    public class EventListenerIAPProduct : EventListenerGeneric<IAPDataVariable>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<IAPDataVariable>[] EventResponses => eventResponses;

        [Serializable]
        public class EventResponse : EventResponse<IAPDataVariable>
        {
            [SerializeField] private ScriptableEventIAPProduct scriptableEvent = null;
            [SerializeField] private IAPProductUnityEvent response = null;

            public override ScriptableEvent<IAPDataVariable> ScriptableEvent => scriptableEvent;
            public override UnityEvent<IAPDataVariable> Response => response;
        }

        [Serializable]
        public class IAPProductUnityEvent : UnityEvent<IAPDataVariable>
        {
        }
    }
}
#endif