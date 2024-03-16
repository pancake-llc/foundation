using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Pancake.Scriptable
{
    /// <summary>
    /// A listener for a ScriptableEventFuncT_TResult.
    /// </summary>
    [EditorIcon("scriptable_event_listener")]
    // ReSharper disable once InconsistentNaming
    public abstract class EventListenerFuncT_TResult<T, TResult> : EventListenerBase
    {
        [System.Serializable]
        public class EventResponse<TV, TR>
        {
            public virtual ScriptableEventFuncT_TResult<TV, TR> ScriptableEvent { get; }
            public virtual UnityEvent<TV> Response { get; }

            [Min(0)] [Tooltip("Delay in seconds before invoking the response.")]
            public float delay;
        }

        protected virtual EventResponse<T, TResult>[] EventResponses { get; }

        private readonly Dictionary<ScriptableEventFuncT_TResult<T, TResult>, EventResponse<T, TResult>> _dictionary = new();

        public void OnEventRaised(ScriptableEventFuncT_TResult<T, TResult> @event, T param, bool debug = false)
        {
            var eventResponse = _dictionary[@event];
            if (eventResponse.delay > 0)
            {
                if (gameObject.activeInHierarchy) StartCoroutine(Cr_DelayInvokeResponse(@event, eventResponse, param, debug));
                else
                    DelayInvokeResponseAsync(@event,
                        eventResponse,
                        param,
                        debug,
                        cancellationTokenSource.Token);
            }
            else
            {
                InvokeResponse(@event, eventResponse, param, debug);
            }
        }

        private IEnumerator Cr_DelayInvokeResponse(ScriptableEventFuncT_TResult<T, TResult> @event, EventResponse<T, TResult> eventResponse, T param, bool debug)
        {
            yield return new WaitForSeconds(eventResponse.delay);
            InvokeResponse(@event, eventResponse, param, debug);
        }

        private async void DelayInvokeResponseAsync(
            ScriptableEventFuncT_TResult<T, TResult> @event,
            EventResponse<T, TResult> eventResponse,
            T param,
            bool debug,
            CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay((int) (eventResponse.delay * 1000), cancellationToken);
                InvokeResponse(@event, eventResponse, param, debug);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void InvokeResponse(ScriptableEventFuncT_TResult<T, TResult> @event, EventResponse<T, TResult> eventResponse, T param, bool debug)
        {
            eventResponse.Response?.Invoke(param);
            if (debug) Debug(@event);
        }

        protected override void ToggleRegistration(bool toggle)
        {
            foreach (var response in EventResponses)
            {
                if (toggle)
                {
                    response.ScriptableEvent.RegisterListener(this);
                    _dictionary.TryAdd(response.ScriptableEvent, response);
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
    }
}