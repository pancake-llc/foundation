using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_string.asset", menuName = "Pancake/Scriptable/Variables/string")]
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
            base.Load();
            Value = Data.Load(Guid, InitialValue);
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (value == PreviousValue) return;
            ValueChanged();
        }
#endif
    }
}