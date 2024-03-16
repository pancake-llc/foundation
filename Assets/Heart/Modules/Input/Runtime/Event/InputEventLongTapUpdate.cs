using System;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "input_event_on_long_tap_update.asset", menuName = "Pancake/Input/Events/on long tap update")]
    public class InputEventLongTapUpdate : ScriptableInput
    {
        private Action<float> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<float> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(float value)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(value);
        }
    }
}