using System;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "scriptable_input_on_update_pinch.asset", menuName = "Pancake/Input/Events/on update pinch")]
    public class ScriptableInputUpdatePinch : ScriptableInput
    {
        private Action<Vector3, float, float> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<Vector3, float, float> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(Vector3 center, float distance, float startDistance)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(center, distance, startDistance);
        }
    }
}