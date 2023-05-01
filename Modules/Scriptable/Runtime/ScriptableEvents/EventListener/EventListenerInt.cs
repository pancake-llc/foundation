using UnityEngine;
using UnityEngine.Events;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/EventListeners/EventListenerInt")]
    public class EventListenerInt : EventListenerGeneric<int>
    {
        [SerializeField] private EventResponse[] _eventResponses = null;
        protected override EventResponse<int>[] EventResponses => _eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<int>
        {
            [SerializeField] private ScriptableEventInt _scriptableEvent = null;
            public override ScriptableEvent<int> ScriptableEvent => _scriptableEvent;

            [SerializeField] private IntUnityEvent _response = null;
            public override UnityEvent<int> Response => _response;
        }
        
        [System.Serializable]
        public class IntUnityEvent : UnityEvent<int>
        {
        }
        
    }
}