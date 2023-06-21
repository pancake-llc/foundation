using UnityEngine;

namespace Pancake.Scriptable
{
    [System.Serializable]
    [EditorIcon("scriptable_variable")]
    public abstract class ScriptableVariableBase : ScriptableBase
    {
        [SerializeField, HideInInspector] private string guid;

        public virtual System.Type GetGenericType { get; }

        public string Guid { get => guid; set => guid = value; }
    }
}