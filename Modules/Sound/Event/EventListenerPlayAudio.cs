using System.Collections.Generic;
using Pancake.Attribute;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Sound
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerPlayAudio : EventListenerBase
    {
        [System.Serializable]
        public struct EventResponse
        {
            public ScriptableEventPlayAudio scriptableEvent;
            public UnityEvent response;
        }

        [SerializeField] private EventResponse[] eventResponses;

        private Dictionary<ScriptableEventPlayAudio, UnityEvent> _dictionary = new Dictionary<ScriptableEventPlayAudio, UnityEvent>();


        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].scriptableEvent.RegisterListener(this);

                    if (!_dictionary.ContainsKey(eventResponses[i].scriptableEvent)) _dictionary.Add(eventResponses[i].scriptableEvent, eventResponses[i].response);
                }
                else
                {
                    eventResponses[i].scriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].scriptableEvent)) _dictionary.Remove(eventResponses[i].scriptableEvent);
                }
            }
        }

        public void OnEventRaised(ScriptableEventPlayAudio eventRaised, bool debug = false)
        {
            _dictionary[eventRaised].Invoke();

            if (debug) Debug(eventRaised);
        }
        
        private void Debug(ScriptableEventPlayAudio eventRaised)
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
                        debugText += r.scriptableEvent.name;
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