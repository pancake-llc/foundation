namespace Pancake
{
    public abstract class CacheComponent<T> : Mono
    {
        public UnityEngine.Transform CachedTransform { get; private set; }
        protected T component;

        protected virtual void Awake()
        {
            if (CachedTransform == null) CachedTransform = transform;
            GetReference();
        }

        protected virtual void Reset() { GetReference(); }

        private void GetReference()
        {
            if (component == null) component = Get<T>();
        }
    }
}