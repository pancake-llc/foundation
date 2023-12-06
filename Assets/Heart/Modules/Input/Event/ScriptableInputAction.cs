using System;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_input")]
    [CreateAssetMenu(fileName = "scriptable_input_on_action.asset", menuName = "Pancake/Input/Events/on action")]
    public class ScriptableInputAction : ScriptableEventBase
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