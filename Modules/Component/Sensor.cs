using Pancake.Apex;
using UnityEngine;

namespace Pancake.Component
{
    public abstract class Sensor : GameComponent
    {
        [Message("Raycast after raycastRate frame\nraycastRate = 1 => Raycast after every frame\nraycastRate = 2 => Raycast every 2 frames", Height = 42)]
        [SerializeField, Range(1, 8)]
        protected int raycastRate = 1;

        public abstract void Pulse();
        public abstract void Stop();
    }
}