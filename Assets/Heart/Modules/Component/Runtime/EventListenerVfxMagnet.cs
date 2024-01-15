using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            [Min(0)] [Tooltip("Delay in seconds before invoking the response.")]
            public float delay;

            public ScriptableEventVfxMagnet vfxMagnetEvent;
            public UnityEvent response;
        }

        [SerializeField] private EventResponse[] eventResponses;
        private readonly Dictionary<ScriptableEventVfxMagnet, EventResponse> _dictionary = new();


        protected override void ToggleRegistration(bool toggle)
        {
            for (var i = 0; i < eventResponses.Length; i++)
            {
                if (toggle)
                {
                    eventResponses[i].vfxMagnetEvent.RegisterListener(this);

                    _dictionary.TryAdd(eventResponses[i].vfxMagnetEvent, eventResponses[i]);
                }
                else
                {
                    eventResponses[i].vfxMagnetEvent.UnregisterListener(this);
                    if (_dictionary.ContainsKey(eventResponses[i].vfxMagnetEvent)) _dictionary.Remove(eventResponses[i].vfxMagnetEvent);
                }
            }
        }

        public void OnEventRaised(ScriptableEventVfxMagnet @event)
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
            foreach (var r in eventResponses)
            {
                var registeredListenerCount = r.response.GetPersistentEventCount();

                for (int i = 0; i < registeredListenerCount; i++)
                {
                    if (r.response.GetPersistentMethodName(i) == methodName)
                    {
                        var sb = new StringBuilder();
                        sb.Append("<color=#f75369>");
                        sb.Append(methodName);
                        sb.Append("()</color> is called by the event: <color=#f75369>");
                        sb.Append("r.vfxMagnetEvent.name</color>");
                        UnityEngine.Debug.Log(sb, gameObject);
                        containsMethod = true;
                        break;
                    }
                }
            }

            return containsMethod;
        }
    }
}