using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_vector2int.asset", menuName = "Pancake/Scriptable/Variables/vector2int")]
    [EditorIcon("scriptable_variable")]
    public class Vector2IntVariable : ScriptableVariable<Vector2Int>
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
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (value == PreviousValue) return;
            ValueChanged();
        }
#endif
    }
}