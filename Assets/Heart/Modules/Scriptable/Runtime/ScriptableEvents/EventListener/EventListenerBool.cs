using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventBool
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerBool")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerBool : EventListenerGeneric<bool>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<bool>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<bool>
        {
            [SerializeField] private ScriptableEventBool scriptableEvent;
            public override ScriptableEvent<bool> ScriptableEvent => scriptableEvent;

            [SerializeField] private BoolUnityEvent response;
            public override UnityEvent<bool> Response => response;
        }
    }
}