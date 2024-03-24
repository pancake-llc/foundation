using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_vector2.asset", menuName = "Pancake/Scriptable/Variables/vector2")]
    [EditorIcon("scriptable_variable")]
    public class Vector2Variable : ScriptableVariable<Vector2>
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