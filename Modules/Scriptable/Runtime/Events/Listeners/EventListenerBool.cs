using Pancake.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerBool : EventListenerGeneric<bool>
    {
        [System.Serializable]
        public class EventResponse : EventResponse<bool>
        {
            [SerializeField] private ScriptableEventBool scriptableEvent;
            [SerializeField] private BoolUnityEvent response;

            public override ScriptableEvent<bool> ScriptableEvent => scriptableEvent;
            public override UnityEvent<bool> Response => response;
        }

        [System.Serializable]
        public class BoolUnityEvent : UnityEvent<bool>
        {
        }

        [SerializeField] private EventResponse[] eventResponses;

        protected override EventResponse<bool>[] EventResponses => eventResponses;
    }
}