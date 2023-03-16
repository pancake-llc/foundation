using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pancake
{
    /// <summary>
    /// Passenger : a person who is travelling in a vehicle but is not driving it, flying it, or working on it
    /// Is the basic unit, Your entities need to inherit this class
    /// </summary>
    public abstract class Passenger : MonoBehaviour, ITickSystem, IFixedTickSystem, ILateTickSystem
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
            Runtime.AddTick(this);
            Runtime.AddFixedTick(this);
            Runtime.AddLateTick(this);
        }

        private void Unsubscribe()
        {
            Runtime.RemoveTick(this);
            Runtime.RemoveFixedTick(this);
            Runtime.RemoveLateTick(this);
        }


        void ITickSystem.OnTick() => Tick();

        void IFixedTickSystem.OnFixedTick() => FixedTick();

        void ILateTickSystem.OnLateTick() => LateTick();


        protected virtual void OnEnabled() { }
        protected virtual void OnDisabled() { }
        protected virtual void Tick() { }
        protected virtual void FixedTick() { }
        protected virtual void LateTick() { }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>() => GetComponent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Gets<T>() => GetComponents<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ChildrenGet<T>() => GetComponentInChildren<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ChildrenGets<T>() => GetComponentsInChildren<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ParentGet<T>() => GetComponentInParent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ParentGets<T>() => GetComponentsInParent<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Find<T>() where T : Object => FindObjectOfType<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] Finds<T>() where T : Object => FindObjectsOfType<T>();
    }
}