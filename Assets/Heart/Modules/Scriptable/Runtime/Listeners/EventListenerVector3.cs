using Pancake.Common;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventVector3.
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerVector3")]
    [EditorIcon("icon_event_listener")]
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
    }
}