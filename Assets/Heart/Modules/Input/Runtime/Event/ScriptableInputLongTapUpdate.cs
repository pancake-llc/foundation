using System;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "scriptable_input_on_long_tap_update.asset", menuName = "Pancake/Input/Events/on long tap update")]
    public class ScriptableInputLongTapUpdate : ScriptableInput
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