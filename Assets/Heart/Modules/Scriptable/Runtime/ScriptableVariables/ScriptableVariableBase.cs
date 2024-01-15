using UnityEngine;

namespace Pancake.Scriptable
{
    [System.Serializable]
    [EditorIcon("scriptable_variable")]
    public abstract class ScriptableVariableBase : ScriptableBase
    {
        [SerializeField] private string guid;
        [SerializeField] private ECreationMode guidCreateMode;

        public virtual System.Type GetGenericType { get; }

        public string Guid { get => guid; set => guid = value; }
        public ECreationMode GuidCreateMode { get => guidCreateMode; set => guidCreateMode = value; }
    }
}