using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerInt")]
    [EditorIcon("scriptable_event_listener")]
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