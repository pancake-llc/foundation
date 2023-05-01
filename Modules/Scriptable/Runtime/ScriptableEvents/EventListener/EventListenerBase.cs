using UnityEngine;

namespace Obvious.Soap
{
    public abstract class EventListenerBase : MonoBehaviour
    {
        protected enum Binding
        {
            UNTIL_DESTROY,
            UNTIL_DISABLE
        }
        
        [SerializeField] protected Binding _binding = Binding.UNTIL_DESTROY;

        [SerializeField] protected bool _disableAfterSubscribing = false;

        protected abstract void ToggleRegistration(bool toggle);

        public abstract bool ContainsCallToMethod(string methodName);
        
        private void Awake()
        {
            if (_binding == Binding.UNTIL_DESTROY)
                ToggleRegistration(true);

            gameObject.SetActive(!_disableAfterSubscribing);
        }

        private void OnEnable()
        {
            if (_binding == Binding.UNTIL_DISABLE)
                ToggleRegistration(true);
        }

        private void OnDisable()
        {
            if (_binding == Binding.UNTIL_DISABLE)
                ToggleRegistration(false);
        }

        private void OnDestroy()
        {
            if (_binding == Binding.UNTIL_DESTROY)
                ToggleRegistration(false);
        }

    }
}