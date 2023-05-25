using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_event_listener")]
    public abstract class EventListenerBase : MonoBehaviour
    {
        protected enum Binding
        {
            UNTIL_DESTROY,
            UNTIL_DISABLE
        }

        [SerializeField] protected Binding binding = Binding.UNTIL_DESTROY;
        [SerializeField] protected bool disableAfterSubscribing = false;

        protected abstract void ToggleRegistration(bool toggle);

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
            if (binding == Binding.UNTIL_DISABLE) ToggleRegistration(false);
        }

        private void OnDestroy()
        {
            if (binding == Binding.UNTIL_DESTROY) ToggleRegistration(false);
        }
    }
}