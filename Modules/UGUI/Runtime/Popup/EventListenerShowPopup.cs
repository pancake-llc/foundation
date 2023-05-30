using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.UI
{
    public class EventListenerShowPopup : EventListenerBase
    {
        [System.Serializable]
        public struct EventResponse
        {
            public PopupShowEvent popupShowEvent;
            public UnityEvent response;
        }

        [SerializeField] private EventResponse[] eventResponses;

        private Dictionary<PopupShowEvent, UnityEvent> _dictionary = new Dictionary<PopupShowEvent, UnityEvent>();

        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].popupShowEvent.RegisterListener(this);

                    _dictionary.TryAdd(eventResponses[i].popupShowEvent, eventResponses[i].response);
                }
                else
                {
                    eventResponses[i].popupShowEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].popupShowEvent)) _dictionary.Remove(eventResponses[i].popupShowEvent);
                }
            }
        }

        public void OnEventRaised(PopupShowEvent eventRaised) { _dictionary[eventRaised].Invoke(); }

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
                        var debugText = $"<color=#f75369>{methodName}()</color>";
                        debugText += " is called by the event: <color=#f75369>";
                        debugText += r.popupShowEvent.name;
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