using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public abstract class EventListenerGeneric<T> : EventListenerBase
    {
        protected virtual EventResponse<T>[] EventResponses { get; }

        private readonly Dictionary<ScriptableEvent<T>, EventResponse<T>> _dictionary = new();

        protected override void ToggleRegistration(bool toggle)
        {
            foreach (var eventResponse in EventResponses)
            {
                if (toggle)
                {
                    eventResponse.ScriptableEvent.RegisterListener(this);
                    _dictionary.TryAdd(eventResponse.ScriptableEvent, eventResponse);
                }
                else
                {
                    eventResponse.ScriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponse.ScriptableEvent)) _dictionary.Remove(eventResponse.ScriptableEvent);
                }
            }
        }

        internal void OnEventRaised(ScriptableEvent<T> eventRaised, T param, bool debug = false)
        {
            var eventResponse = _dictionary[eventRaised];
            if (eventResponse.delay > 0)
            {
                if (gameObject.activeInHierarchy) StartCoroutine(Cr_DelayInvokeResponse(eventRaised, eventResponse, param, debug));
                else
                    DelayInvokeResponseAsync(eventRaised,
                        eventResponse,
                        param,
                        debug,
                        cancellationTokenSource.Token);
            }
            else InvokeResponse(eventRaised, eventResponse, param, debug);
        }

        private IEnumerator Cr_DelayInvokeResponse(ScriptableEvent<T> eventRaised, EventResponse<T> eventResponse, T param, bool debug)
        {
            yield return new WaitForSeconds(eventResponse.delay);
            InvokeResponse(eventRaised, eventResponse, param, debug);
        }

        private async void DelayInvokeResponseAsync(
            ScriptableEvent<T> eventRaised,
            EventResponse<T> eventResponse,
            T param,
            bool debug,
            CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay((int) (eventResponse.delay * 1000), cancellationToken);
                InvokeResponse(eventRaised, eventResponse, param, debug);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void InvokeResponse(ScriptableEvent<T> eventRaised, EventResponse<T> eventResponse, T param, bool debug)
        {
            eventResponse.Response?.Invoke(param);
            if (debug) Debug(eventRaised);
        }

        private void Debug(ScriptableEvent<T> eventRaised)
        {
            var response = _dictionary[eventRaised].Response;
            var registeredListenerCount = response.GetPersistentEventCount();

            for (var i = 0; i < registeredListenerCount; i++)
            {
                var sb = new StringBuilder();
                sb.Append("<color=#f75369>[Event] </color>");
                sb.Append(eventRaised.name);
                sb.Append(" => ");
                sb.Append(response.GetPersistentTarget(i).name);
                sb.Append(".");
                sb.Append(response.GetPersistentMethodName(i));
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
                        sb.Append($"<color=#f75369>{methodName}()</color>");
                        sb.Append(" is called by: <color=#f75369>[Event]</color> ");
                        sb.Append(eventResponse.ScriptableEvent.name);
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

            [Min(0)] [Tooltip("Delay in seconds before invoking the response.")]
            public float delay;
        }
    }
}