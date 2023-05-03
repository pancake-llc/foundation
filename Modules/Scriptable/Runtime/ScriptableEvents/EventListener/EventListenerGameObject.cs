using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerGameObject")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerGameObject : EventListenerGeneric<GameObject>
    {
        [SerializeField] private EventResponse[] m_eventResponses = null;
        protected override EventResponse<GameObject>[] EventResponses => m_eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<GameObject>
        {
            [SerializeField] private ScriptableEventGameObject mScriptableEvent = null;
            public override ScriptableEvent<GameObject> ScriptableEvent => mScriptableEvent;

            [SerializeField] private GameObjectUnityEvent m_response = null;
            public override UnityEvent<GameObject> Response => m_response;
        }

        [System.Serializable]
        public class GameObjectUnityEvent : UnityEvent<GameObject>
        {
        }
    }
}