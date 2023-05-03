using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerBool")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerBool : EventListenerGeneric<bool>
    {
        [SerializeField] private EventResponse[] m_eventResponses = null;
        protected override EventResponse<bool>[] EventResponses => m_eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<bool>
        {
            [SerializeField] private ScriptableEventBool mScriptableEvent = null;
            public override ScriptableEvent<bool> ScriptableEvent => mScriptableEvent;

            [SerializeField] private BoolUnityEvent m_response = null;
            public override UnityEvent<bool> Response => m_response;
        }

        [System.Serializable]
        public class BoolUnityEvent : UnityEvent<bool>
        {
        }
    }
}