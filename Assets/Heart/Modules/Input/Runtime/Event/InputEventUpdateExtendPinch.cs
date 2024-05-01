using System;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("so_blue_input")]
    [CreateAssetMenu(fileName = "input_event_on_update_extend_pinch.asset", menuName = "Pancake/Input/Events/on update extend pinch")]
    public class InputEventUpdateExtendPinch : ScriptableInput
    {
        private Action<PinchData> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<PinchData> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(PinchData data)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(data);
        }
    }
}