using Pancake.Attribute;

namespace Pancake.Scriptable
{
    using UnityEngine;

    [EditorIcon("scriptable_variable")]
    [CreateAssetMenu(fileName = "scriptable_variable_color.asset", menuName = "Pancake/Scriptable/Variables/color")]
    [System.Serializable]
    public class ColorVariable : ScriptableVariable<Color>
    {
        public override void Save()
        {
            Data.Save(Id, Value);
            base.Save();
        }

        public override void Load()
        {
            Value = Data.Load(Id, InitialValue);
            base.Load();
        }

        public void SetRandom()
        {
            var beautifulColor = UnityEngine.Random.ColorHSV(0f,
                1f,
                1f,
                1f,
                0.5f,
                1f);
            Value = beautifulColor;
        }
    }
}