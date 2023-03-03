using Pancake.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerGameObject : EventListenerGeneric<GameObject>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<GameObject>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<GameObject>
        {
            [SerializeField] private ScriptableEventGameObject scriptableEvent;
            [SerializeField] private GameObjectUnityEvent response;

            public override ScriptableEvent<GameObject> ScriptableEvent => scriptableEvent;
            public override UnityEvent<GameObject> Response => response;
        }

        [System.Serializable]
        public class GameObjectUnityEvent : UnityEvent<GameObject>
        {
        }
    }
}