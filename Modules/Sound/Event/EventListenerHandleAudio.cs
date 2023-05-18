using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Sound
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerHandleAudio : EventListenerFunc<AudioHandle, bool>
    {
        [SerializeField] private EventResponse[] eventResponses;

        protected override EventResponse<AudioHandle, bool>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<AudioHandle, bool>
        {
            [SerializeField] private AudioHandleEvent audioHandleEvent;
            [SerializeField] private AudioFinishUnityEvent response;

            public override ScriptableEventFunc<AudioHandle, bool> ScriptableEvent => audioHandleEvent;
            public override UnityEvent<AudioHandle> Response => response;
        }


        [System.Serializable]
        public class AudioFinishUnityEvent : UnityEvent<AudioHandle>
        {
        }
    }
}