using Pancake.Attribute;
using UnityEngine;

namespace Pancake.Scriptable
{
    [EditorIcon("scriptable_variable")]
    [CreateAssetMenu(fileName = "scriptable_variable_bool.asset", menuName = "Pancake/Scriptable/Variables/bool")]
    [System.Serializable]
    public class BoolVariable : ScriptableVariable<bool>
    {
        public override void Save()
        {
            Data.Save(Id, Value ? 1 : 0);
            base.Save();
        }

        public override void Load()
        {
            Value = Data.Load(Id, InitialValue ? 1 : 0) == 1;
            base.Load();
        }

        public void Toggle() { Value = !Value; }
    }
}