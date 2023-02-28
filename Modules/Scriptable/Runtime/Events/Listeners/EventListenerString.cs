using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    public class EventListenerString : EventListenerGeneric<string>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<string>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<string>
        {
            [SerializeField] private ScriptableEventString scriptableEvent;
            [SerializeField] private StringUnityEvent response;

            public override ScriptableEvent<string> ScriptableEvent => scriptableEvent;
            public override UnityEvent<string> Response => response;
        }

        [System.Serializable]
        public class StringUnityEvent : UnityEvent<string>
        {
        }
    }
}