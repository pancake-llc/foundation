using Pancake.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerColor : EventListenerGeneric<Color>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<Color>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Color>
        {
            [SerializeField] private ScriptableEventColor scriptableEvent;
            [SerializeField] private ColorUnityEvent response;

            public override ScriptableEvent<Color> ScriptableEvent => scriptableEvent;
            public override UnityEvent<Color> Response => response;
        }

        [System.Serializable]
        public class ColorUnityEvent : UnityEvent<Color>
        {
        }
    }
}