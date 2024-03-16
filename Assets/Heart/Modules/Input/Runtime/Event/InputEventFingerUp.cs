using System;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "input_event_on_finger_up.asset", menuName = "Pancake/Input/Events/on finger up")]
    public class InputEventFingerUp : ScriptableInput
    {
        private Action _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise()
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke();
        }
    }
}