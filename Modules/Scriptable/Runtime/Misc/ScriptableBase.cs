using UnityEngine;

namespace Pancake.Scriptable
{
    [Searchable]
    [System.Serializable]
    public abstract class ScriptableBase : ScriptableObject
    {
        public virtual void Reset() { }
        public System.Action repaintRequest;
    }
}