using UnityEngine;

namespace Pancake.Scriptable
{
    [System.Serializable]
    public abstract class ScriptableEventBase : ScriptableBase
    {
        [Tooltip("Enable console logs when this event is raised.")] [SerializeField]
        protected bool debugLogEnabled;

        public bool DebugLogEnabled => debugLogEnabled;

        public virtual System.Type GetGenericType { get; }
    }
}