using Pancake.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerVector2 : EventListenerGeneric<Vector2>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<Vector2>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Vector2>
        {
            [SerializeField] private ScriptableEventVector2 scriptableEvent;
            [SerializeField] private Vector2UnityEvent response;

            public override ScriptableEvent<Vector2> ScriptableEvent => scriptableEvent;
            public override UnityEvent<Vector2> Response => response;
        }

        [System.Serializable]
        public class Vector2UnityEvent : UnityEvent<Vector2>
        {
        }
    }
}