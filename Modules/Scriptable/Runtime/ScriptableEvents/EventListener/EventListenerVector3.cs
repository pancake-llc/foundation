using UnityEngine;
using UnityEngine.Events;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/EventListeners/EventListenerVector3")]
    public class EventListenerVector3 : EventListenerGeneric<Vector3>
    {
        [SerializeField] private EventResponse[] m_eventResponses = null;
        protected override EventResponse<Vector3>[] EventResponses => m_eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Vector3>
        {
            [SerializeField] private ScriptableEventVector3 mScriptableEvent = null;
            public override ScriptableEvent<Vector3> ScriptableEvent => mScriptableEvent;

            [SerializeField] private Vector3UnityEvent m_response = null;
            public override UnityEvent<Vector3> Response => m_response;
        }
        
        [System.Serializable]
        public class Vector3UnityEvent : UnityEvent<Vector3>
        {
        }
    }
}