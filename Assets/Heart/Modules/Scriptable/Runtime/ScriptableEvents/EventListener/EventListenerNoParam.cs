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
    /// A listener for a ScriptableEventNoParam.
    /// </summary>
    [AddComponentMenu("Scriptable/EventListeners/EventListenerNoParam")]
    [EditorIcon("scriptable_event_listener")]
    public class EventListenerNoParam : EventListenerBase
    {
        [SerializeField] private EventResponse[] eventResponses;

        private readonly Dictionary<ScriptableEventNoParam, EventResponse> _dictionary = new();

        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].ScriptableEvent.RegisterListener(this);
                    _dictionary.TryAdd(eventResponses[i].ScriptableEvent, eventResponses[i]);
                }
                else
                {
                    eventResponses[i].ScriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].ScriptableEvent)) _dictionary.Remove(eventResponses[i].ScriptableEvent);
                }
            }
        }

        public void OnEventRaised(ScriptableEventNoParam @event, bool debug = false)
        {
            var eventResponse = _dictionary[@event];
            if (eventResponse.Delay > 0)
            {
                if (gameObject.activeInHierarchy) StartCoroutine(Cr_DelayInvokeResponse(@event, eventResponse, debug));
                else DelayInvokeResponseAsync(@event, eventResponse, debug, cancellationTokenSource.Token);
            }
            else InvokeResponse(@event, eventResponse, debug);
        }

        private IEnumerator Cr_DelayInvokeResponse(ScriptableEventNoParam eventRaised, EventResponse eventResponse, bool debug)
        {
            yield return new WaitForSeconds(eventResponse.Delay);
            InvokeResponse(eventRaised, eventResponse, debug);
        }

        private async void DelayInvokeResponseAsync(ScriptableEventNoParam eventRaised, EventResponse eventResponse, bool debug, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay((int) (eventResponse.Delay * 1000), cancellationToken);
                InvokeResponse(eventRaised, eventResponse, debug);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void InvokeResponse(ScriptableEventNoParam eventRaised, EventResponse eventResponse, bool debug)
        {
            eventResponse.Response?.Invoke();
            if (debug) Debug(eventRaised);
        }

        [System.Serializable]
        public struct EventResponse
        {
            [Min(0)] [Tooltip("Delay in seconds before invoking the response.")]
            public float Delay;

            public ScriptableEventNoParam ScriptableEvent;
            public UnityEvent Response;
        }

        #region Debugging

        private void Debug(ScriptableEventNoParam eventRaised)
        {
            var response = _dictionary[eventRaised].Response;
            int registeredListenerCount = response.GetPersistentEventCount();

            for (var i = 0; i < registeredListenerCount; i++)
            {
                var sb = new StringBuilder();
                sb.Append("<color=#f75369>[Event] ");
                sb.Append(eventRaised.name);
                sb.Append(" => </color>");
                sb.Append(response.GetPersistentTarget(i));
                sb.Append(".");
                sb.Append(response.GetPersistentMethodName(i));
                sb.Append("()");
                UnityEngine.Debug.Log(sb.ToString(), gameObject);
            }
        }

        public override bool ContainsCallToMethod(string methodName)
        {
            var containsMethod = false;
            foreach (var eventResponse in eventResponses)
            {
                var registeredListenerCount = eventResponse.Response.GetPersistentEventCount();

                for (int i = 0; i < registeredListenerCount; i++)
                {
                    if (eventResponse.Response.GetPersistentMethodName(i) == methodName)
                    {
                        var sb = new StringBuilder();
                        sb.Append("<color=#f75369>");
                        sb.Append(methodName);
                        sb.Append("()</color> is called by the event: <color=#f75369>");
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

        #endregion
    }
}