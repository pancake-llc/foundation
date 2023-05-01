using UnityEngine;

namespace Obvious.Soap
{
    [System.Serializable]
    public abstract class ScriptableVariableBase : ScriptableBase
    {
        [SerializeField, HideInInspector] private string _guid;

        public string Guid
        {
            get => _guid;
            set => _guid = value;
        }
    }
}