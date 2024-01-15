#if PANCAKE_IAP
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pancake.Scriptable;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Pancake.IAP
{
    [EditorIcon("scriptable_event_listener")]
    [AddComponentMenu("Scriptable/EventListeners/EventListenerIAPNoParam")]
    public class EventListenerIAPNoParam : EventListenerBase
    {
        [SerializeField] private EventResponse[] eventResponses;
        private readonly Dictionary<ScriptableEventIAPNoParam, EventResponse> _dictionary = new();

        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].scriptableEvent.RegisterListener(this);

                    if (!_dictionary.ContainsKey(eventResponses[i].scriptableEvent)) _dictionary.Add(eventResponses[i].scriptableEvent, eventResponses[i]);
                }
                else
                {
                    eventResponses[i].scriptableEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].scriptableEvent)) _dictionary.Remove(eventResponses[i].scriptableEvent);
                }
            }
        }

        public void OnEventRaised(ScriptableEventIAPNoParam @event)
        {
            var eventResponse = _dictionary[@event];
            if (eventResponse.delay > 0)
            {
                if (gameObject.activeInHierarchy) StartCoroutine(Cr_DelayInvokeResponse(eventResponse));
                else DelayInvokeResponseAsync(eventResponse, cancellationTokenSource.Token);
            }
            else InvokeResponse(eventResponse);
        }

        private IEnumerator Cr_DelayInvokeResponse(EventResponse eventResponse)
        {
            yield return new WaitForSeconds(eventResponse.delay);
            InvokeResponse(eventResponse);
        }

        private async void DelayInvokeResponseAsync(EventResponse eventResponse, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay((int) (eventResponse.delay * 1000), cancellationToken);
                InvokeResponse(eventResponse);
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void InvokeResponse(EventResponse eventResponse) { eventResponse.response?.Invoke(); }

        public override bool ContainsCallToMethod(string methodName)
        {
            var containsMethod = false;
            foreach (var eventResponse in eventResponses)
            {
                var registeredListenerCount = eventResponse.response.GetPersistentEventCount();

                for (int i = 0; i < registeredListenerCount; i++)
                {
                    if (eventResponse.response.GetPersistentMethodName(i) == methodName)
                    {
                        var debugText = $"<color=#f75369>{methodName}()</color>";
                        debugText += " is called by the event: <color=#f75369>";
                        debugText += eventResponse.scriptableEvent.name;
                        debugText += "</color>";
                        Debug.Log(debugText, gameObject);
                        containsMethod = true;
                        break;
                    }
                }
            }

            return containsMethod;
        }


        [Serializable]
        public struct EventResponse
        {
            [Min(0)] [Tooltip("Delay in seconds before invoking the response.")]
            public float delay;

            public ScriptableEventIAPNoParam scriptableEvent;
            public UnityEvent response;
        }
    }
}
#endif