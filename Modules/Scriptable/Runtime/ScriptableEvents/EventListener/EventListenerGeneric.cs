using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;

namespace Obvious.Soap
{
    public abstract class EventListenerGeneric<T> : EventListenerBase
    {
        protected virtual EventResponse<T>[] EventResponses { get; }

        private readonly Dictionary<ScriptableEvent<T>, UnityEvent<T>> _dictionary =
            new Dictionary<ScriptableEvent<T>, UnityEvent<T>>();

        public void OnEventRaised(ScriptableEvent<T> scriptableEventRaised, T param, bool debug = false)
        {
            _dictionary[scriptableEventRaised].Invoke(param);

            if (debug)
                Debug(scriptableEventRaised);
        }

        protected override void ToggleRegistration(bool toggle)
        {
            foreach (var t in EventResponses)
            {
                if (toggle)
                {
                    t.ScriptableEvent.RegisterListener(this);
                    if (!_dictionary.ContainsKey(t.ScriptableEvent))
                        _dictionary.Add(t.ScriptableEvent, t.Response);
                }
                else
                {
                    t.ScriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(t.ScriptableEvent))
                        _dictionary.Remove(t.ScriptableEvent);
                }
            }
        }

        private void Debug(ScriptableEvent<T> eventRaised)
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

        [System.Serializable]
        public class EventResponse<U>
        {
            public virtual ScriptableEvent<U> ScriptableEvent { get; }
            public virtual UnityEvent<U> Response { get; }
        }
    }

    
}