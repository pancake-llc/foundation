using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerVector3")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerVector3 : EventListenerGeneric<Vector3>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<Vector3>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Vector3>
        {
            [SerializeField] private ScriptableEventVector3 scriptableEvent;
            public override ScriptableEvent<Vector3> ScriptableEvent => scriptableEvent;

            [SerializeField] private Vector3UnityEvent response;
            public override UnityEvent<Vector3> Response => response;
        }

        [System.Serializable]
        public class Vector3UnityEvent : UnityEvent<Vector3>
        {
        }
    }
}