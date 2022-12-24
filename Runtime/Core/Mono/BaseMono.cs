using System;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// Base class MonoBehavior
    /// </summary>
    public class BaseMono : MonoAllocation, ITickSystem, IFixedTickSystem, ILateTickSystem
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
            Runtime.tickSystems.Add(this);
            Runtime.fixedTickSystems.Add(this);
            Runtime.lateTickSystems.Add(this);
        }

        private void Unsubscribe()
        {
            Runtime.tickSystems.Remove(this);
            Runtime.fixedTickSystems.Remove(this);
            Runtime.lateTickSystems.Remove(this);
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