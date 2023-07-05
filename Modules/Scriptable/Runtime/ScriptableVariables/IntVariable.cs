using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_int.asset", menuName = "Pancake/Scriptable/Variables/int")]
    [EditorIcon("scriptable_variable")]
    public class IntVariable : ScriptableVariable<int>
    {
        [Tooltip("Clamps the value of this variable to a minimum and maximum.")] [SerializeField]
        private bool isClamped;

        public bool IsClamped => isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField] [ShowIf(nameof(isClamped), true)]
        private Vector2Int minMax = new Vector2Int(int.MinValue, int.MaxValue);

        public Vector2Int MinMax { get => minMax; set => minMax = value; }

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

        public void Add(int value) { Value += value; }

        public override int Value
        {
            get => value;
            set
            {
                var clampedValue = IsClamped ? Mathf.Clamp(value, minMax.x, minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (IsClamped)
            {
                var clampedValue = Mathf.Clamp(value, minMax.x, minMax.y);
                if (value < clampedValue || value > clampedValue)
                    value = clampedValue;
            }

            base.OnValidate();
        }
#endif
    }
}