using Pancake.Common;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventString.
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerString")]
    [EditorIcon("icon_event_listener")]
    public class EventListenerString : EventListenerGeneric<string>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<string>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<string>
        {
            [SerializeField] private ScriptableEventString scriptableEvent;
            public override ScriptableEvent<string> ScriptableEvent => scriptableEvent;

            [SerializeField] private StringUnityEvent response;
            public override UnityEvent<string> Response => response;
        }
    }
}