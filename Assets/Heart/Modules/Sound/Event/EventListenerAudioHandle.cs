using System;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Sound
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerAudioHandle : EventListenerGeneric<AudioHandle>
    {
        [SerializeField] private EventResponse[] eventResponses;
        protected override EventResponse<AudioHandle>[] EventResponses => eventResponses;

        [Serializable]
        public class EventResponse : EventResponse<AudioHandle>
        {
            [SerializeField] private ScriptableEventAudioHandle scriptableEvent;
            public override ScriptableEvent<AudioHandle> ScriptableEvent => scriptableEvent;

            [SerializeField] private AudioHandleEvent response;
            public override UnityEvent<AudioHandle> Response => response;
        }

        [Serializable]
        public class AudioHandleEvent : UnityEvent<AudioHandle>
        {
        }
    }
}