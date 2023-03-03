using System.Collections.Generic;
using Pancake.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerNoParam : EventListenerBase
    {
        [System.Serializable]
        public struct EventResponse
        {
            public ScriptableEventNoParam ScriptableEvent;
            public UnityEvent Response;
        }

        [SerializeField] private EventResponse[] eventResponses;

        private Dictionary<ScriptableEventNoParam, UnityEvent> _dictionary = new Dictionary<ScriptableEventNoParam, UnityEvent>();

        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].ScriptableEvent.RegisterListener(this);

                    if (!_dictionary.ContainsKey(eventResponses[i].ScriptableEvent)) _dictionary.Add(eventResponses[i].ScriptableEvent, eventResponses[i].Response);
                }
                else
                {
                    eventResponses[i].ScriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].ScriptableEvent)) _dictionary.Remove(eventResponses[i].ScriptableEvent);
                }
            }
        }
        
        public void OnEventRaised(ScriptableEventNoParam eventRaised, bool debug = false)
        {
            _dictionary[eventRaised].Invoke();

            if (debug) Debug(eventRaised);
        }
        
        private void Debug(ScriptableEventNoParam eventRaised)
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
                var registeredListenerCount = r.Response.GetPersistentEventCount();

                for (int i = 0; i < registeredListenerCount; i++)
                {
                    if (r.Response.GetPersistentMethodName(i) == methodName)
                    {
                        var debugText = $"<color=#52D5F2>{methodName}()</color>";
                        debugText += " is called by the event: <color=#52D5F2>";
                        debugText += r.ScriptableEvent.name;
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