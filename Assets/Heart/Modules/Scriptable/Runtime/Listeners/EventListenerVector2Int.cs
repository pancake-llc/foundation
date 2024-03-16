using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventVector2Int.
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerVector2Int")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerVector2Int : EventListenerGeneric<Vector2Int>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<Vector2Int>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Vector2Int>
        {
            [SerializeField] private ScriptableEventVector2Int scriptableEvent;
            public override ScriptableEvent<Vector2Int> ScriptableEvent => scriptableEvent;

            [SerializeField] private Pancake.Vector2IntUnityEvent response;
            public override UnityEvent<Vector2Int> Response => response;
        }
    }
}