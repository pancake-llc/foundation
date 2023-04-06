using UnityEngine;

namespace Pancake.Scriptable
{
    public abstract class EventListenerBase : MonoBehaviour
    {
        protected enum Binding
        {
            UntilDestroy,
            UntilDisable
        }

        [SerializeField] protected Binding binding = Binding.UntilDestroy;
        [SerializeField] protected bool disableAfterSubscribing;

        protected abstract void ToggleRegistration(bool toggle);
        public abstract bool ContainsCallToMethod(string methodName);

        private void Awake()
        {
            if (binding == Binding.UntilDestroy) ToggleRegistration(true);
            gameObject.SetActive(!disableAfterSubscribing);
        }

        private void OnEnable()
        {
            if (binding == Binding.UntilDisable) ToggleRegistration(true);
        }

        private void OnDisable()
        {
            if (binding == Binding.UntilDisable) ToggleRegistration(false);
        }

        private void OnDestroy()
        {
            if (binding == Binding.UntilDestroy) ToggleRegistration(false);
        }
    }
}