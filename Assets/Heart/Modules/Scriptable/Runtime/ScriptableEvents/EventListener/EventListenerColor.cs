using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerColor")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerColor : EventListenerGeneric<Color>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<Color>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Color>
        {
            [SerializeField] private ScriptableEventColor scriptableEvent;
            public override ScriptableEvent<Color> ScriptableEvent => scriptableEvent;

            [SerializeField] private ColorUnityEvent response;
            public override UnityEvent<Color> Response => response;
        }

        [System.Serializable]
        public class ColorUnityEvent : UnityEvent<Color>
        {
        }
    }
}