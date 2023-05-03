using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerVector2")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerVector2 : EventListenerGeneric<Vector2>
    {
        [SerializeField] private EventResponse[] m_eventResponses = null;
        protected override EventResponse<Vector2>[] EventResponses => m_eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Vector2>
        {
            [SerializeField] private ScriptableEventVector2 mScriptableEvent = null;
            public override ScriptableEvent<Vector2> ScriptableEvent => mScriptableEvent;

            [SerializeField] private Vector2UnityEvent m_response = null;
            public override UnityEvent<Vector2> Response => m_response;
        }

        [System.Serializable]
        public class Vector2UnityEvent : UnityEvent<Vector2>
        {
        }
    }
}