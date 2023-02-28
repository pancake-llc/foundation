using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_float.asset", menuName = "Pancake/Scriptable Variable/float")]
    [System.Serializable]
    public class FloatVariable : ScriptableVariable<float>
    {
        [SerializeField] private bool isClamped;

        public bool IsClamped => isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField] [ShowIf("isClamped", true)]
        private Vector2 minMax = new Vector2(float.MinValue, float.MaxValue);

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

        public void Add(float value) { Value += value; }

        public override float Value
        {
            get => value;
            set
            {
                float clampedValue = isClamped ? value.Clamp(minMax.x, minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (isClamped)
            {
                float clampedValue = value.Clamp(minMax.x, minMax.y);
                if (value < clampedValue || value > clampedValue) value = clampedValue;
            }

            base.OnValidate();
        }
#endif
    }
}