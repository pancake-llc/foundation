using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerString")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerString : EventListenerGeneric<string>
    {
        [SerializeField] private EventResponse[] m_eventResponses = null;
        protected override EventResponse<string>[] EventResponses => m_eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<string>
        {
            [SerializeField] private ScriptableEventString mScriptableEvent = null;
            public override ScriptableEvent<string> ScriptableEvent => mScriptableEvent;

            [SerializeField] private StringUnityEvent m_response = null;
            public override UnityEvent<string> Response => m_response;
        }

        [System.Serializable]
        public class StringUnityEvent : UnityEvent<string>
        {
        }
    }
}