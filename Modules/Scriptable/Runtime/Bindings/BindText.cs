using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Obvious.Soap
{
    [AddComponentMenu("Soap/Bindings/BindText")]
    [RequireComponent(typeof(Text))]
    public class BindText: CacheComponent<Text>
    {
        public CustomVariableType Type = CustomVariableType.None;
        
        [SerializeField] private BoolVariable _boolVariable = null;
        [SerializeField] private IntVariable _intVariable = null;
        [SerializeField] private FloatVariable _floatVariable = null;
        [SerializeField] private StringVariable _stringVariable = null;

        public string Prefix = string.Empty;
        public string Suffix = string.Empty;

        //int specific
        [Tooltip("Useful too an offset, for example for Level counts. If your level index is  0, add 1, so it displays Level : 1")]
        public int Increment = 0;
        [Tooltip("Clamps the value shown to a minimum and a maximum.")]
        public Vector2Int MinMaxInt = new Vector2Int(int.MinValue, int.MaxValue);
        
        //float specific
        [Min(1)]
        public int DecimalAmount = 2;
        [Tooltip("Clamps the value shown to a minimum and a maximum.")]
        public bool IsClamped = false;
        public Vector2 MinMaxFloat = new Vector2(float.MinValue, float.MaxValue);
        
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        
        protected override void Awake()
        {
            base.Awake();
            if (Type == CustomVariableType.None)
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
            _stringBuilder.Append(Prefix);

            switch (Type)
            {
                case CustomVariableType.Bool:
                    _stringBuilder.Append(_boolVariable.Value ? "True" : "False");
                    break;
                case CustomVariableType.Int:
                    var clampedInt = IsClamped ? Mathf.Clamp(_intVariable.Value, MinMaxInt.x, MinMaxInt.y) : _intVariable.Value;
                    _stringBuilder.Append(clampedInt + Increment);
                    break;
                case CustomVariableType.Float:
                    double clampedFloat = IsClamped ? Mathf.Clamp(_floatVariable.Value, MinMaxFloat.x, MinMaxFloat.y) : _floatVariable.Value;
                    double rounded = System.Math.Round(clampedFloat, DecimalAmount);
                    _stringBuilder.Append(rounded);
                    break;
                case CustomVariableType.String:
                    _stringBuilder.Append(_stringVariable.Value);
                    break;
            }

            _stringBuilder.Append(Suffix);
            _component.text = _stringBuilder.ToString();
        }

        private void Subscribe()
        {
            switch (Type)
            {
                case CustomVariableType.Bool:
                    if (_boolVariable != null)
                        _boolVariable.OnValueChanged += (value)=> Refresh();
                    break;
                case CustomVariableType.Int:
                    if (_intVariable != null)
                        _intVariable.OnValueChanged += (value)=> Refresh();
                    break;
                case CustomVariableType.Float:
                    if (_floatVariable != null)
                        _floatVariable.OnValueChanged += (value)=> Refresh();
                    break;
                case CustomVariableType.String:
                    if (_stringVariable != null)
                        _stringVariable.OnValueChanged += (value)=> Refresh();
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (Type)
            {
                case CustomVariableType.Bool:
                    if (_boolVariable != null)
                        _boolVariable.OnValueChanged -= (value)=> Refresh();
                    break;
                case CustomVariableType.Int:
                    if (_intVariable != null)
                        _intVariable.OnValueChanged -= (value)=> Refresh();
                    break;
                case CustomVariableType.Float:
                    if (_floatVariable != null)
                        _floatVariable.OnValueChanged -= (value)=> Refresh();
                    break;
                case CustomVariableType.String:
                    if (_stringVariable != null)
                        _stringVariable.OnValueChanged -= (value)=> Refresh();
                    break;
            }
        }

    }
}