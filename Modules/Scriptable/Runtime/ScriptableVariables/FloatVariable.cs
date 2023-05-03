using Pancake.Apex;
using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_float.asset", menuName = "Pancake/Scriptable/ScriptableVariables/float")]
    [EditorIcon("scriptable_variable")]
    public class FloatVariable : ScriptableVariable<float>
    {
        [SerializeField] private bool _isClamped = false;
        public bool IsClamped => _isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField] [ShowIf("_isClamped", true)]
        private Vector2 _minMax = new Vector2(float.MinValue, float.MaxValue);

        public Vector2 MinMax { get => _minMax; set => _minMax = value; }

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

        public void Add(float value) { Value += value; }

        public override float Value
        {
            get => _value;
            set
            {
                var clampedValue = _isClamped ? Mathf.Clamp(value, _minMax.x, _minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (_isClamped)
            {
                var clampedValue = Mathf.Clamp(_value, _minMax.x, _minMax.y);
                if (_value < clampedValue || _value > clampedValue)
                    _value = clampedValue;
            }

            base.OnValidate();
        }
#endif
    }
}