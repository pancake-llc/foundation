using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_vector3.asset", menuName = "Pancake/Scriptable/Variables/vector3")]
    [EditorIcon("scriptable_variable")]
    public class Vector3Variable : ScriptableVariable<Vector3>
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
    }
}