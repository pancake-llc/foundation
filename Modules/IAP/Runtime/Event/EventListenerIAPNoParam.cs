#if PANCAKE_IAP
using System;
using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event_listener")]
    [AddComponentMenu("Scriptable/EventListeners/EventListenerIAPNoParam")]
    public class EventListenerIAPNoParam : EventListenerBase
    {
        [SerializeField] private EventResponse[] _eventResponses = null;
        private Dictionary<ScriptableEventIAPNoParam, UnityEvent> _dictionary = new Dictionary<ScriptableEventIAPNoParam, UnityEvent>();

        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < _eventResponses.Length; i++)
            {
                if (toggle)
                {
                    _eventResponses[i].ScriptableEvent.RegisterListener(this);

                    if (!_dictionary.ContainsKey(_eventResponses[i].ScriptableEvent))
                        _dictionary.Add(_eventResponses[i].ScriptableEvent, _eventResponses[i].Response);
                }
                else
                {
                    _eventResponses[i].ScriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(_eventResponses[i].ScriptableEvent))
                        _dictionary.Remove(_eventResponses[i].ScriptableEvent);
                }
            }
        }

        public void OnEventRaised(ScriptableEventIAPNoParam eventRaised) { _dictionary[eventRaised].Invoke(); }

        public override bool ContainsCallToMethod(string methodName)
        {
            var containsMethod = false;
            foreach (var eventResponse in _eventResponses)
            {
                var registeredListenerCount = eventResponse.Response.GetPersistentEventCount();

                for (int i = 0; i < registeredListenerCount; i++)
                {
                    if (eventResponse.Response.GetPersistentMethodName(i) == methodName)
                    {
                        var debugText = $"<color=#f75369>{methodName}()</color>";
                        debugText += " is called by the event: <color=#f75369>";
                        debugText += eventResponse.ScriptableEvent.name;
                        debugText += "</color>";
                        Debug.Log(debugText, gameObject);
                        containsMethod = true;
                        break;
                    }
                }
            }

            return containsMethod;
        }


        [Serializable]
        public struct EventResponse
        {
            public ScriptableEventIAPNoParam ScriptableEvent;
            public UnityEvent Response;
        }
    }
}
#endif