using Pancake.Apex;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.Component
{
    public abstract class Sensor : GameComponent
    {
        [Message("Raycast after raycastRate frame\nraycastRate = 1 => Raycast after every frame\nraycastRate = 2 => Raycast every 2 frames", Height = 42)]
        [SerializeField, Range(1, 8)]
        protected int raycastRate = 1;

        [SerializeField] protected LayerMask layer;

        [SerializeField] protected ScriptableEventNoParam pulseEvent;
        [SerializeField] protected ScriptableEventNoParam stopEvent;
        
        protected bool isPlaying;

        protected virtual void Register()
        {
            if (pulseEvent != null) pulseEvent.OnRaised += Pulse;
            if (stopEvent != null) stopEvent.OnRaised += Stop;
        }

        protected virtual void Unregister()
        {
            if (pulseEvent != null) pulseEvent.OnRaised -= Pulse;
            if (stopEvent != null) stopEvent.OnRaised -= Stop;
        }

        protected override void OnEnabled() { Register(); }

        protected override void OnDisabled() { Unregister(); }

        protected abstract void Pulse();

        protected virtual void Stop() { isPlaying = false; }
    }
}