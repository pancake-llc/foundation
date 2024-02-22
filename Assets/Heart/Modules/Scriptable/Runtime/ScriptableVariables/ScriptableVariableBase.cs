using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_variable")]
    public abstract class ScriptableVariableBase : ScriptableBase
    {
        [SerializeField] private string guid;
        [SerializeField] private ECreationMode guidCreateMode;

        public string Guid { get => guid; set => guid = value; }
        public ECreationMode GuidCreateMode { get => guidCreateMode; set => guidCreateMode = value; }
    }
}