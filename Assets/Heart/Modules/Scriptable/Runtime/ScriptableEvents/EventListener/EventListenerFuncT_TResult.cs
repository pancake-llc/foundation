using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    // ReSharper disable once InconsistentNaming
    public abstract class EventListenerFuncT_TResult<T, TResult> : EventListenerBase
    {
        [System.Serializable]
        public class EventResponse<TV, TR>
        {
            public virtual ScriptableEventFuncT_TResult<TV, TR> ScriptableEvent { get; }
            public virtual UnityEvent<TV> Response { get; }
        }

        protected virtual EventResponse<T, TResult>[] EventResponses { get; }

        private readonly Dictionary<ScriptableEventFuncT_TResult<T, TResult>, UnityEvent<T>> _dictionary = new Dictionary<ScriptableEventFuncT_TResult<T, TResult>, UnityEvent<T>>();

        public void OnEventRaised(ScriptableEventFuncT_TResult<T, TResult> scriptableEvent, T param, bool debug = false)
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

        private void Debug(ScriptableEventFuncT_TResult<T, TResult> eventRaised)
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