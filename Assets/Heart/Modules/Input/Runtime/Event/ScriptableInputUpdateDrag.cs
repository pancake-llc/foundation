using System;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "scriptable_input_on_update_drag.asset", menuName = "Pancake/Input/Events/on update drag")]
    public class ScriptableInputUpdateDrag : ScriptableInput
    {
        private Action<Vector3, Vector3, Vector3, Vector3> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<Vector3, Vector3, Vector3, Vector3> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(Vector3 startPosition, Vector3 currentPosition, Vector3 correctionOffset, Vector3 delta)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(startPosition, currentPosition, correctionOffset, delta);
        }
    }
}