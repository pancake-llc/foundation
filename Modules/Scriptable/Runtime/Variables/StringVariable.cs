using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_variable")]
    [CreateAssetMenu(fileName = "scriptable_variable_string.asset", menuName = "Pancake/Scriptable/Variables/string")]
    [System.Serializable]
    public class StringVariable : ScriptableVariable<string>
    {
        public override void Save()
        {
            Data.Save(Id, Value);
            base.Save();
        }

        public override void Load()
        {
            Value = Data.Load(Id, InitialValue);
            base.Load();
        }
    }
}