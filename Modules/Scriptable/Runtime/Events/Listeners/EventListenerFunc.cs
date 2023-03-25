using System.Collections.Generic;
using System.Text;
using Pancake.Attribute;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public abstract class EventListenerFunc<T, TRsult> : EventListenerBase
    {
        [System.Serializable]
        public class EventResponse<TV, TR>
        {
            public virtual ScriptableEventFunc<TV, TR> ScriptableEvent { get; }
            public virtual UnityEvent<TV> Response { get; }
        }
        
        protected virtual EventResponse<T, TRsult>[] EventResponses { get; }

        private readonly Dictionary<ScriptableEventFunc<T, TRsult>, UnityEvent<T>> _dictionary = new Dictionary<ScriptableEventFunc<T, TRsult>, UnityEvent<T>>();

        public void OnEventRaised(ScriptableEventFunc<T, TRsult> scriptableEvent, T param, bool debug = false)
        {
            _dictionary[scriptableEvent]?.Invoke(param);
            if (debug) Debug(scriptableEvent);
        }

        protected override void ToggleRegistration(bool toggle)
        {
            foreach (var response in EventResponses)
            {
                if (toggle)
                {
                    response.ScriptableEvent.RegisterListener(this);
                    if (!_dictionary.ContainsKey(response.ScriptableEvent)) _dictionary.Add(response.ScriptableEvent, response.Response);
                }
                else
                {
                    response.ScriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(response.ScriptableEvent)) _dictionary.Remove(response.ScriptableEvent);
                }
            }
        }

        public override bool ContainsCallToMethod(string methodName)
        {
            var containsMethod = false;
            foreach (var eventResponse in EventResponses)
            {
                int registeredListenerCount = eventResponse.Response.GetPersistentEventCount();

                for (var i = 0; i < registeredListenerCount; i++)
                {
                    if (eventResponse.Response.GetPersistentMethodName(i) == methodName)
                    {
                        var sb = new StringBuilder();
                        sb.Append($"<color=#52D5F2>{methodName}()</color>");
                        sb.Append(" is called by: <color=#52D5F2>[Event] ");
                        sb.Append(eventResponse.ScriptableEvent.name);
                        sb.Append("</color>");
                        UnityEngine.Debug.Log(sb.ToString(), gameObject);
                        containsMethod = true;
                        break;
                    }
                }
            }

            return containsMethod;
        }

        private void Debug(ScriptableEventFunc<T, TRsult> eventRaised)
        {
            var listener = _dictionary[eventRaised];
            var registeredListenerCount = listener.GetPersistentEventCount();

            for (var i = 0; i < registeredListenerCount; i++)
            {
                var sb = new StringBuilder();
                sb.Append($"<color=#52D5F2>[Event] </color>");
                sb.Append(eventRaised.name);
                sb.Append(" => ");
                sb.Append(listener.GetPersistentTarget(i).name);
                sb.Append(".");
                sb.Append(listener.GetPersistentMethodName(i));
                sb.Append("()");
                UnityEngine.Debug.Log(sb.ToString(), gameObject);
            }
        }
    }

}