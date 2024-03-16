using System;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "input_event_on_start_drag.asset", menuName = "Pancake/Input/Events/on start drag")]
    public class InputEventStartDrag : ScriptableInput
    {
        private Action<Vector3, bool> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<Vector3, bool> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(Vector3 position, bool isLongTap)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(position, isLongTap);
        }
    }
}