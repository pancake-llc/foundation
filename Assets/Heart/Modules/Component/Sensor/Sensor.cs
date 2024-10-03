using Sirenix.OdinInspector;
using UnityEngine;

namespace Pancake.Component
{
    public abstract class Sensor : GameComponent
    {
        [InfoBox("Raycast after raycastRate frame\nraycastRate = 1 => Raycast after every frame\nraycastRate = 2 => Raycast every 2 frames")]
        [SerializeField, Range(1, 8)]
        protected int raycastRate = 1;

        [SerializeField] protected LayerMask layer;

        protected bool isPlaying;

        public abstract void Pulse();

        public virtual void Stop() { isPlaying = false; }
    }
}