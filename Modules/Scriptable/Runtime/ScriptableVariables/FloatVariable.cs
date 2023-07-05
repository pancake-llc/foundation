using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_float.asset", menuName = "Pancake/Scriptable/Variables/float")]
    [EditorIcon("scriptable_variable")]
    public class FloatVariable : ScriptableVariable<float>
    {
        [Tooltip("Clamps the value of this variable to a minimum and maximum.")] [SerializeField]
        private bool isClamped;

        public bool IsClamped => isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField] [ShowIf(nameof(isClamped), true)]
        private Vector2 minMax = new Vector2(float.MinValue, float.MaxValue);

        public Vector2 MinMax { get => minMax; set => minMax = value; }

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

        public void Add(float value) { Value += value; }

        public override float Value
        {
            get => value;
            set
            {
                float clampedValue = isClamped ? Mathf.Clamp(value, minMax.x, minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (isClamped)
            {
                float clampedValue = Mathf.Clamp(value, minMax.x, minMax.y);
                if (value < clampedValue || value > clampedValue) value = clampedValue;
            }

            base.OnValidate();
        }
#endif
    }
}