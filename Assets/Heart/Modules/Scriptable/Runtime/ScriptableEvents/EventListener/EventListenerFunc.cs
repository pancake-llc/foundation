using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public abstract class EventListenerFunc<TResult> : EventListenerBase
    {
        [System.Serializable]
        public class EventResponse<TR>
        {
            public virtual ScriptableEventFunc<TR> ScriptableEvent { get; }
            public virtual UnityEvent Response { get; }
        }
        protected virtual EventResponse<TResult>[] EventResponses { get; }
        private readonly Dictionary<ScriptableEventFunc<TResult>, UnityEvent> _dictionary = new Dictionary<ScriptableEventFunc<TResult>, UnityEvent>();

        public void OnEventRaised(ScriptableEventFunc<TResult> scriptableEvent, bool debug = false)
        {
            _dictionary[scriptableEvent]?.Invoke();
            if (debug) Debug(scriptableEvent);
        }

        protected override void ToggleRegistration(bool toggle)
        {
            foreach (var response in EventResponses)
            {
                if (toggle)
                {
                    response.ScriptableEvent.RegisterListener(this);
                    _dictionary.TryAdd(response.ScriptableEvent, response.Response);
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
                        sb.Append($"<color=#f75369>{methodName}()</color>");
                        sb.Append(" is called by: <color=#f75369>[Event] ");
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
        
        private void Debug(ScriptableEventFunc<TResult> eventRaised)
        {
            var listener = _dictionary[eventRaised];
            var registeredListenerCount = listener.GetPersistentEventCount();

            for (var i = 0; i < registeredListenerCount; i++)
            {
                var sb = new StringBuilder();
                sb.Append("<color=#f75369>[Event] </color>");
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