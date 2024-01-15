using System;
using Pancake.Scriptable;
using UnityEngine;

namespace Pancake.MobileInput
{
    [EditorIcon("scriptable_event")]
    [CreateAssetMenu(fileName = "scriptable_event_vector3, transform.asset", menuName = "Pancake/Input/Events/vector3_transform")]
    public class ScriptableEventVector3Transform : ScriptableEventBase
    {
        private Action<Vector3, Transform> _onRaised;

        /// <summary>
        /// Action raised when this event is raised.
        /// </summary>
        public event Action<Vector3, Transform> OnRaised { add => _onRaised += value; remove => _onRaised -= value; }

        /// <summary>
        /// Raise the event
        /// </summary>
        internal void Raise(Vector3 p, Transform t)
        {
            if (!Application.isPlaying) return;
            _onRaised?.Invoke(p, t);
        }
    }
}