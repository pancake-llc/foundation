using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_bool.asset", menuName = "Pancake/Scriptable/Variables/bool")]
    [System.Serializable]
    [EditorIcon("scriptable_variable")]
    public class BoolVariable : ScriptableVariable<bool>
    {
        public override void Save()
        {
            Data.Save(Guid, Value);
            base.Save();
        }

        public override void Load()
        {
            Value = Data.Load(Guid, DefaultValue);
            base.Load();
        }

        public void Toggle() { Value = !Value; }
    }
}