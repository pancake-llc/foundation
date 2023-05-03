using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Pancake.Scriptable
{
    [AddComponentMenu("Scriptable/Bindings/BindText")]
    [RequireComponent(typeof(Text))]
    public class BindText : CacheGameComponent<Text>
    {
        public CustomVariableType type = CustomVariableType.None;

        [SerializeField] private BoolVariable boolVariable = null;
        [SerializeField] private IntVariable intVariable = null;
        [SerializeField] private FloatVariable floatVariable = null;
        [SerializeField] private StringVariable stringVariable = null;

        public string prefix = string.Empty;
        public string suffix = string.Empty;

        //int specific
        [Tooltip("Useful too an offset, for example for Level counts. If your level index is  0, add 1, so it displays Level : 1")]
        public int increment = 0;

        [Tooltip("Clamps the value shown to a minimum and a maximum.")]
        public Vector2Int minMaxInt = new Vector2Int(int.MinValue, int.MaxValue);

        //float specific
        [Min(1)] public int decimalAmount = 2;

        [Tooltip("Clamps the value shown to a minimum and a maximum.")]
        public bool isClamped = false;

        public Vector2 minMaxFloat = new Vector2(float.MinValue, float.MaxValue);

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        protected override void Awake()
        {
            base.Awake();
            if (type == CustomVariableType.None)
            {
                Debug.LogError("Select a type for this binding component", gameObject);
                return;
            }

            Refresh();
            Subscribe();
        }

        private void Refresh()
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(prefix);

            switch (type)
            {
                case CustomVariableType.Bool:
                    _stringBuilder.Append(boolVariable.Value ? "True" : "False");
                    break;
                case CustomVariableType.Int:
                    var clampedInt = isClamped ? Mathf.Clamp(intVariable.Value, minMaxInt.x, minMaxInt.y) : intVariable.Value;
                    _stringBuilder.Append(clampedInt + increment);
                    break;
                case CustomVariableType.Float:
                    double clampedFloat = isClamped ? Mathf.Clamp(floatVariable.Value, minMaxFloat.x, minMaxFloat.y) : floatVariable.Value;
                    double rounded = System.Math.Round(clampedFloat, decimalAmount);
                    _stringBuilder.Append(rounded);
                    break;
                case CustomVariableType.String:
                    _stringBuilder.Append(stringVariable.Value);
                    break;
            }

            _stringBuilder.Append(suffix);
            component.text = _stringBuilder.ToString();
        }

        private void Subscribe()
        {
            switch (type)
            {
                case CustomVariableType.Bool:
                    if (boolVariable != null)
                        boolVariable.OnValueChanged += (value) => Refresh();
                    break;
                case CustomVariableType.Int:
                    if (intVariable != null)
                        intVariable.OnValueChanged += (value) => Refresh();
                    break;
                case CustomVariableType.Float:
                    if (floatVariable != null)
                        floatVariable.OnValueChanged += (value) => Refresh();
                    break;
                case CustomVariableType.String:
                    if (stringVariable != null)
                        stringVariable.OnValueChanged += (value) => Refresh();
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (type)
            {
                case CustomVariableType.Bool:
                    if (boolVariable != null)
                        boolVariable.OnValueChanged -= (value) => Refresh();
                    break;
                case CustomVariableType.Int:
                    if (intVariable != null)
                        intVariable.OnValueChanged -= (value) => Refresh();
                    break;
                case CustomVariableType.Float:
                    if (floatVariable != null)
                        floatVariable.OnValueChanged -= (value) => Refresh();
                    break;
                case CustomVariableType.String:
                    if (stringVariable != null)
                        stringVariable.OnValueChanged -= (value) => Refresh();
                    break;
            }
        }
    }
}