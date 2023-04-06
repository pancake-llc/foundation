using System.Collections.Generic;
using Pancake.Attribute;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Pancake.Sound
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerPlayAudio : EventListenerBase
    {
        [System.Serializable]
        public struct EventResponse
        {
            [FormerlySerializedAs("scriptableEvent")] public AudioPlayEvent audioPlayEvent;
            public UnityEvent response;
        }

        [SerializeField] private EventResponse[] eventResponses;

        private Dictionary<AudioPlayEvent, UnityEvent> _dictionary = new Dictionary<AudioPlayEvent, UnityEvent>();


        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].audioPlayEvent.RegisterListener(this);

                    if (!_dictionary.ContainsKey(eventResponses[i].audioPlayEvent)) _dictionary.Add(eventResponses[i].audioPlayEvent, eventResponses[i].response);
                }
                else
                {
                    eventResponses[i].audioPlayEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].audioPlayEvent)) _dictionary.Remove(eventResponses[i].audioPlayEvent);
                }
            }
        }

        public void OnEventRaised(AudioPlayEvent eventRaised, bool debug = false)
        {
            _dictionary[eventRaised].Invoke();

            if (debug) Debug(eventRaised);
        }
        
        private void Debug(AudioPlayEvent eventRaised)
        {
            var listener = _dictionary[eventRaised];
            var registeredListenerCount = listener.GetPersistentEventCount();

            for (var i = 0; i < registeredListenerCount; i++)
            {
                var debugText = $"<color=#52D5F2>[Event] ";
                debugText += eventRaised.name;
                debugText += " => </color>";
                debugText += listener.GetPersistentTarget(i);
                debugText += ".";
                debugText += listener.GetPersistentMethodName(i);
                debugText += "()";
                UnityEngine.Debug.Log(debugText, gameObject);
            }
        }

        public override bool ContainsCallToMethod(string methodName)
        {
            var containsMethod = false;
            foreach (var r in eventResponses)
            {
                var registeredListenerCount = r.response.GetPersistentEventCount();

                for (int i = 0; i < registeredListenerCount; i++)
                {
                    if (r.response.GetPersistentMethodName(i) == methodName)
                    {
                        var debugText = $"<color=#52D5F2>{methodName}()</color>";
                        debugText += " is called by the event: <color=#52D5F2>";
                        debugText += r.audioPlayEvent.name;
                        debugText += "</color>";
                        UnityEngine.Debug.Log(debugText, gameObject);
                        containsMethod = true;
                        break;
                    }
                }
            }

            return containsMethod;
        }
    }
}