using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Sound
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerAudio : EventListenerFuncT_TResult<Audio, AudioHandle>
    {
        [SerializeField] private EventResponse[] eventResponses;

        protected override EventResponse<Audio, AudioHandle>[] EventResponses => eventResponses;

        [System.Serializable]
        public class EventResponse : EventResponse<Audio, AudioHandle>
        {
            [SerializeField] private ScriptableEventAudio audioEvent;
            [SerializeField] private AudioFinishUnityEvent response;

            public override ScriptableEventFuncT_TResult<Audio, AudioHandle> ScriptableEvent => audioEvent;
            public override UnityEvent<Audio> Response => response;
        }


        [System.Serializable]
        public class AudioFinishUnityEvent : UnityEvent<Audio>
        {
        }
    }
}