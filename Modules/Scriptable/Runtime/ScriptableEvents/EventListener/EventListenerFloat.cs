using UnityEngine;
using UnityEngine.Events;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/EventListeners/EventListenerFloat")]
    public class EventListenerFloat : EventListenerGeneric<float>
    {
        [SerializeField] private EventResponse[] m_eventResponses = null;
        protected override EventResponse<float>[] EventResponses => m_eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<float>
        {
            [SerializeField] private ScriptableEventFloat mScriptableEvent = null;
            public override ScriptableEvent<float> ScriptableEvent => mScriptableEvent;

            [SerializeField] private FloatUnityEvent m_response = null;
            public override UnityEvent<float> Response => m_response;
        }

        [System.Serializable]
        public class FloatUnityEvent : UnityEvent<float>
        {
            
        }
    }
}