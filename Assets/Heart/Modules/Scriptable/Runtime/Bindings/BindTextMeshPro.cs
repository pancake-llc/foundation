using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using TMPro;

namespace Pancake.Scriptable
{
    /// <summary>
    /// Binds a variable to a TextMeshPro component
    /// </summary>
    [AddComponentMenu("Scriptable/Bindings/BindTextMeshPro")]
    [RequireComponent(typeof(TMP_Text))]
    public class BindTextMeshPro : CacheGameComponent<TMP_Text>
    {
        public CustomVariableType type = CustomVariableType.None;

        [SerializeField] private BoolVariable boolVariable;
        [SerializeField] private IntVariable intVariable;
        [SerializeField] private FloatVariable floatVariable;
        [SerializeField] private StringVariable stringVariable;

        [Tooltip("Displays before the value")] public string prefix = string.Empty;
        [Tooltip("Displays after the value")] public string suffix = string.Empty;

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
                    if (boolVariable != null) boolVariable.OnValueChanged += OnValueChanged;
                    break;
                case CustomVariableType.Int:
                    if (intVariable != null) intVariable.OnValueChanged += OnValueChanged;
                    break;
                case CustomVariableType.Float:
                    if (floatVariable != null) floatVariable.OnValueChanged += OnValueChanged;
                    break;
                case CustomVariableType.String:
                    if (stringVariable != null) stringVariable.OnValueChanged += OnValueChanged;
                    break;
            }
        }

        private void OnDestroy()
        {
            switch (type)
            {
                case CustomVariableType.Bool:
                    if (boolVariable != null) boolVariable.OnValueChanged -= OnValueChanged;
                    break;
                case CustomVariableType.Int:
                    if (intVariable != null) intVariable.OnValueChanged -= OnValueChanged;
                    break;
                case CustomVariableType.Float:
                    if (floatVariable != null) floatVariable.OnValueChanged -= OnValueChanged;
                    break;
                case CustomVariableType.String:
                    if (stringVariable != null) stringVariable.OnValueChanged -= OnValueChanged;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnValueChanged(string _) { Refresh(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnValueChanged(float _) { Refresh(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnValueChanged(int _) { Refresh(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnValueChanged(bool _) { Refresh(); }
    }
}