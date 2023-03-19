using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake
{
    /// <summary>
    /// Passenger : a person who is travelling in a vehicle but is not driving it, flying it, or working on it
    /// Is the basic unit, Your entities need to inherit this class
    /// </summary>
    public abstract class Passenger : Entity, ITickSystem, IFixedTickSystem, ILateTickSystem
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
        
        void ITickSystem.OnTick() => Tick();

        void IFixedTickSystem.OnFixedTick() => FixedTick();

        void ILateTickSystem.OnLateTick() => LateTick();


        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void Tick() { }
        protected virtual void FixedTick() { }
        protected virtual void LateTick() { }
    }
}