using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_string.asset", menuName = "Pancake/Scriptable Variable/string")]
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
            Value = Data.Load(Id, initialValue);
            base.Load();
        }
    }
}