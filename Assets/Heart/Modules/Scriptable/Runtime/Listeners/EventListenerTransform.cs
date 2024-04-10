using Pancake.Common;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventTransform
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerTransform")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerTransform : EventListenerGeneric<Transform>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<Transform>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Transform>
        {
            [SerializeField] private ScriptableEventTransform scriptableEvent;
            public override ScriptableEvent<Transform> ScriptableEvent => scriptableEvent;

            [SerializeField] private TransformUnityEvent response;
            public override UnityEvent<Transform> Response => response;
        }
    }
}