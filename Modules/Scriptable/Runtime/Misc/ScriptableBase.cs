using UnityEngine;

namespace Obvious.Soap
{
    [System.Serializable]
    public abstract class ScriptableBase : ScriptableObject
    {
        public virtual void Reset() { }
    }
}