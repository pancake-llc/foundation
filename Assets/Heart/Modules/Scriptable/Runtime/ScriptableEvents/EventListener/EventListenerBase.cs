using System.Threading;
using UnityEngine;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Base class for all event listeners
    /// </summary>
    [EditorIcon("scriptable_event_listener")]
    public abstract class EventListenerBase : MonoBehaviour
    {
        protected enum Binding
        {
            UNTIL_DESTROY,
            UNTIL_DISABLE
        }

        [SerializeField] protected Binding binding = Binding.UNTIL_DESTROY;
        [SerializeField] protected bool disableAfterSubscribing;
        protected readonly CancellationTokenSource cancellationTokenSource = new();

        protected abstract void ToggleRegistration(bool toggle);

        /// <summary>
        /// Returns true if the event listener contains a call to the method with the given name
        /// </summary>
        public abstract bool ContainsCallToMethod(string methodName);

        private void Awake()
        {
            if (binding == Binding.UNTIL_DESTROY) ToggleRegistration(true);
            gameObject.SetActive(!disableAfterSubscribing);
        }

        private void OnEnable()
        {
            if (binding == Binding.UNTIL_DISABLE) ToggleRegistration(true);
        }

        private void OnDisable()
        {
            if (binding == Binding.UNTIL_DISABLE)
            {
                ToggleRegistration(false);
                cancellationTokenSource.Cancel();
            }
        }

        private void OnDestroy()
        {
            if (binding == Binding.UNTIL_DESTROY)
            {
                ToggleRegistration(false);
                cancellationTokenSource.Cancel();
            }
        }
    }
}