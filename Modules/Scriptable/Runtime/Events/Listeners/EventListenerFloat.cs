using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    public class EventListenerFloat : EventListenerGeneric<float>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<float>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<float>
        {
            [SerializeField] private ScriptableEventFloat scriptableEvent;
            [SerializeField] private FloatUnityEvent response;

            public override ScriptableEvent<float> ScriptableEvent => scriptableEvent;
            public override UnityEvent<float> Response => response;
        }

        [System.Serializable]
        public class FloatUnityEvent : UnityEvent<float>
        {
        }
    }
}