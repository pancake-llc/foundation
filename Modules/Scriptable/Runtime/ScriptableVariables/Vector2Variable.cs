using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_vector2.asset", menuName = "Pancake/Scriptable/ScriptableVariables/vector2")]
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
            Value = Data.Load(Guid, InitialValue);
            base.Load();
        }
    }
}