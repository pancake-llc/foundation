using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;


namespace Pancake.IAP
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerIAPData : EventListenerGeneric<IAPDataVariable>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<IAPDataVariable>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<IAPDataVariable>
        {
            [SerializeField] private ScriptableEventIAPData scriptableEvent = null;
            [SerializeField] private IAPDataUnityEvent response = null;

            public override ScriptableEvent<IAPDataVariable> ScriptableEvent => scriptableEvent;
            public override UnityEvent<IAPDataVariable> Response => response;
        }

        [System.Serializable]
        public class IAPDataUnityEvent : UnityEvent<IAPDataVariable>
        {
        }
    }
}