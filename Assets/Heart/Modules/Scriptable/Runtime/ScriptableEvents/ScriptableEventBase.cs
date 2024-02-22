using UnityEngine;

namespace Pancake.Scriptable
{
    public abstract class ScriptableEventBase : ScriptableBase
    {
        [Tooltip("Enable console logs when this event is raised.")] [SerializeField]
        protected bool debugLogEnabled;

        public bool DebugLogEnabled => debugLogEnabled;
    }
}