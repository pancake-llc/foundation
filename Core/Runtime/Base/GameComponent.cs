namespace Pancake
{
    /// <summary>
    /// Passenger : a person who is travelling in a vehicle but is not driving it, flying it, or working on it
    /// Is the basic unit, Your entities need to inherit this class
    /// </summary>
    public abstract class GameComponent : UnityEngine.MonoBehaviour, ITickProcess, IFixedTickProcess, ILateTickProcess, IComponent
    {
        private void OnEnable()
        {
            OnEnabled();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            OnDisabled();
        }

        private void Subscribe()
        {
            App.AddTick(this);
            App.AddFixedTick(this);
            App.AddLateTick(this);
        }

        private void Unsubscribe()
        {
            App.RemoveTick(this);
            App.RemoveFixedTick(this);
            App.RemoveLateTick(this);
        }

        void ITickProcess.OnTick() => Tick();

        void IFixedTickProcess.OnFixedTick() => FixedTick();

        void ILateTickProcess.OnLateTick() => LateTick();


        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void Tick() { }
        protected virtual void FixedTick() { }
        protected virtual void LateTick() { }
        public UnityEngine.GameObject GameObject => gameObject;
        public UnityEngine.Transform Transform => transform;
    }
}