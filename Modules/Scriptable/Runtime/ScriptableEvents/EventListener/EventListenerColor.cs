using UnityEngine;
using UnityEngine.Events;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/EventListeners/EventListenerColor")]
    public class EventListenerColor : EventListenerGeneric<Color>
    {
        [SerializeField] private EventResponse[] m_eventResponses = null;
        protected override EventResponse<Color>[] EventResponses => m_eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Color>
        {
            [SerializeField] private ScriptableEventColor mScriptableEvent = null;
            public override ScriptableEvent<Color> ScriptableEvent => mScriptableEvent;

            [SerializeField] private ColorUnityEvent m_response = null;
            public override UnityEvent<Color> Response => m_response;
        }
        
        [System.Serializable]
        public class ColorUnityEvent : UnityEvent<Color>
        {
        }
    }
}