namespace Pancake
{
    /// <summary>
    /// Cache component with type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CacheGameComponent<T> : GameComponent
    {
        public UnityEngine.Transform CachedTransform { get; private set; }
        public T component;

        protected virtual void Awake()
        {
            if (CachedTransform == null) CachedTransform = transform;
            GetReference();
        }

        protected virtual void Reset() { GetReference(); }

        private void GetReference()
        {
            if (component == null) component = GetComponent<T>();
        }
    }
}