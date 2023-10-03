using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerFloat")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerFloat : EventListenerGeneric<float>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<float>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<float>
        {
            [SerializeField] private ScriptableEventFloat scriptableEvent;
            public override ScriptableEvent<float> ScriptableEvent => scriptableEvent;

            [SerializeField] private FloatUnityEvent response;
            public override UnityEvent<float> Response => response;
        }

        [System.Serializable]
        public class FloatUnityEvent : UnityEvent<float>
        {
        }
    }
}