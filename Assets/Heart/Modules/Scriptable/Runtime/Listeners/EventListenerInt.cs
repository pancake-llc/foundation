using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventInt.
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerInt")]
    [EditorIcon("icon_event_listener")]
    public class EventListenerInt : EventListenerGeneric<int>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<int>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<int>
        {
            [SerializeField] private ScriptableEventInt scriptableEvent;
            public override ScriptableEvent<int> ScriptableEvent => scriptableEvent;

            [SerializeField] private Pancake.Common.IntUnityEvent response;
            public override UnityEvent<int> Response => response;
        }
    }
}