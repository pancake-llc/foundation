using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_string.asset", menuName = "Pancake/Scriptable/ScriptableVariables/string")]
    [EditorIcon("scriptable_variable")]
    public class StringVariable : ScriptableVariable<string>
    {
        public override void Save()
        {
            Data.Save(Guid, Value);
            base.Save();
        }

        public override void Load()
        {
            Value = Data.Load(Guid, InitialValue);
            base.Load();
        }
    }
}