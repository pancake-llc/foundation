namespace Pancake.Scriptable
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "scriptable_variable_vector3.asset", menuName = "Pancake/Scriptable Variable/vector3")]
    [System.Serializable]
    public class Vector3Variable : ScriptableVariable<Vector3>
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