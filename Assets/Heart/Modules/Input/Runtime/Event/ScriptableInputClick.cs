using System;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "scriptable_input_on_click.asset", menuName = "Pancake/Input/Events/on click")]
    public class ScriptableInputClick : ScriptableInput
    {
        private Action<Vector3, bool, bool> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<Vector3, bool, bool> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }
        
        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(Vector3 position, bool isDoubleClick, bool isLongTap)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(position, isDoubleClick, isLongTap);
        }
    }
}