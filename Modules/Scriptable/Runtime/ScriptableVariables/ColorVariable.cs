using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_color.asset", menuName = "Pancake/Scriptable/Variables/color")]
    [EditorIcon("scriptable_variable")]
    public class ColorVariable : ScriptableVariable<Color>
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

        public void SetRandom()
        {
            var beautifulColor = Random.ColorHSV(0f,
                1f,
                1f,
                1f,
                0.5f,
                1f);
            Value = beautifulColor;
        }
    }
}