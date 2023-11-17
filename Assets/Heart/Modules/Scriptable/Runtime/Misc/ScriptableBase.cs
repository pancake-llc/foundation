using UnityEngine;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Base classes of all ScriptableObjects
    /// </summary>
    [Searchable]
    [System.Serializable]
    public abstract class ScriptableBase : ScriptableObject
    {
        public virtual void Reset() { }
        public System.Action repaintRequest;
    }
}