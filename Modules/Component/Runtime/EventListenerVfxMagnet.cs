using System.Collections.Generic;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Component
{
    [AddComponentMenu("Scriptable/EventListeners/EventListenerVfxMagnet")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerVfxMagnet : EventListenerBase
    {
        [System.Serializable]
        public struct EventResponse
        {
            public ScriptableEventVfxMagnet vfxMagnetEvent;
            public UnityEvent response;
        }

        [SerializeField] private EventResponse[] eventResponses;
        private Dictionary<ScriptableEventVfxMagnet, UnityEvent> _dictionary = new Dictionary<ScriptableEventVfxMagnet, UnityEvent>();


        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].vfxMagnetEvent.RegisterListener(this);

                    _dictionary.TryAdd(eventResponses[i].vfxMagnetEvent, eventResponses[i].response);
                }
                else
                {
                    eventResponses[i].vfxMagnetEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].vfxMagnetEvent)) _dictionary.Remove(eventResponses[i].vfxMagnetEvent);
                }
            }
        }

        public void OnEventRaised(ScriptableEventVfxMagnet eventRaised) { _dictionary[eventRaised].Invoke(); }

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
                        debugText += r.vfxMagnetEvent.name;
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