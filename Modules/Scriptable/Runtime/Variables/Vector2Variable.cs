namespace Pancake.Scriptable
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "scriptable_variable_vector2.asset", menuName = "Pancake/ScriptableVariables/vector2")]
    [System.Serializable]
    public class Vector2Variable : ScriptableVariable<Vector2>
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