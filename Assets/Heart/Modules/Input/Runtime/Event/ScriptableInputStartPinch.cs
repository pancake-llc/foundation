using System;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "scriptable_input_on_start_pinch.asset", menuName = "Pancake/Input/Events/on start pinch")]
    public class ScriptableInputStartPinch : ScriptableInput
    {
        private Action<Vector3, float> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<Vector3, float> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(Vector3 center, float distance)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(center, distance);
        }
    }
}