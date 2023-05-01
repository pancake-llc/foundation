using UnityEngine;

namespace Obvious.Soap
{
    [CreateAssetMenu(fileName = "scriptable_variable_color.asset", menuName = "Soap/ScriptableVariables/color")]
    public class ColorVariable : ScriptableVariable<Color>
    {
        public override void Save()
        {
            PlayerPrefs.SetFloat(Guid + "_r", Value.r);
            PlayerPrefs.SetFloat(Guid + "_g", Value.g);
            PlayerPrefs.SetFloat(Guid + "_b", Value.b);
            PlayerPrefs.SetFloat(Guid + "_a", Value.a);
            base.Save();
        }

        public override void Load()
        {
            var r = PlayerPrefs.GetFloat(Guid + "_r", InitialValue.r);
            var g = PlayerPrefs.GetFloat(Guid + "_g", InitialValue.g);
            var b = PlayerPrefs.GetFloat(Guid + "_b", InitialValue.b);
            var a = PlayerPrefs.GetFloat(Guid + "_a", InitialValue.a);
            Value = new Color(r, g, b, a);
            base.Load();
        }

        public void SetRandom()
        {
            var beautifulColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            Value = beautifulColor;
        }
    }
}