namespace Pancake
{
    /// <summary>
    /// Use as an alternative to MonoBehaviour using a managed player loop.
    /// </summary>
    public abstract class GameComponent : UnityEngine.MonoBehaviour, IComponent
    {
        private void OnEnable()
        {
            OnEnabled();
           
        }

        private void OnDisable()
        {
          
            OnDisabled();
        }
        

        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void Tick() { }
        protected virtual void FixedTick() { }
        protected virtual void LateTick() { }
        public UnityEngine.GameObject GameObject => gameObject;
        public UnityEngine.Transform Transform => transform;
    }
}