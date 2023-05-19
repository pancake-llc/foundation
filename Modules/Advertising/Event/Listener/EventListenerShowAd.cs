using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Monetization
{
    public class EventListenerShowAd : EventListenerBase
    {
        [System.Serializable]
        public struct EventResponse
        {
            public ScriptableEventShowAd scriptableEvent;
            public UnityEvent response;
        }

        [SerializeField] private EventResponse[] eventResponses;

        private Dictionary<ScriptableEventShowAd, UnityEvent> _dictionary = new Dictionary<ScriptableEventShowAd, UnityEvent>();

        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].scriptableEvent.RegisterListener(this);

                    _dictionary.TryAdd(eventResponses[i].scriptableEvent, eventResponses[i].response);
                }
                else
                {
                    eventResponses[i].scriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].scriptableEvent)) _dictionary.Remove(eventResponses[i].scriptableEvent);
                }
            }
        }

        public void OnEventRaised(ScriptableEventShowAd eventRaised) { _dictionary[eventRaised].Invoke(); }

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