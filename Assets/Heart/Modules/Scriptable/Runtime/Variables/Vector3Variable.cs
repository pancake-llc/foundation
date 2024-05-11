using UnityEngine;
using Pancake.Common;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_vector3.asset", menuName = "Pancake/Scriptable/Variables/vector3")]
    [EditorIcon("so_blue_variable")]
    public class Vector3Variable : ScriptableVariable<Vector3>
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