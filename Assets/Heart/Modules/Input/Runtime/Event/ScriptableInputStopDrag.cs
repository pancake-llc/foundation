using System;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "scriptable_input_on_stop_drag.asset", menuName = "Pancake/Input/Events/on stop drag")]
    public class ScriptableInputStopDrag : ScriptableInput
    {
        private Action<Vector3, Vector3> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<Vector3, Vector3> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(Vector3 stopPosition, Vector3 finalMomentum)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(stopPosition, finalMomentum);
        }
    }
}