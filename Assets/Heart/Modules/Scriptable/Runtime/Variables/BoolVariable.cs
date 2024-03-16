using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_bool.asset", menuName = "Pancake/Scriptable/Variables/bool")]
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

        /// <summary>
        /// Use this to toggle the value of the variable.
        /// </summary>
        public void Toggle() { Value = !Value; }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (value == PreviousValue) return;
            ValueChanged();
        }
#endif
    }
}