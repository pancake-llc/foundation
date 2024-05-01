using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventVector2.
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerVector2")]
    [EditorIcon("icon_event_listener")]
    public class EventListenerVector2 : EventListenerGeneric<Vector2>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<Vector2>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Vector2>
        {
            [SerializeField] private ScriptableEventVector2 scriptableEvent;
            public override ScriptableEvent<Vector2> ScriptableEvent => scriptableEvent;

            [SerializeField] private Pancake.Common.Vector2UnityEvent response;
            public override UnityEvent<Vector2> Response => response;
        }
    }
}