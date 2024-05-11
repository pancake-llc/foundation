using Alchemy.Inspector;
using UnityEngine;
using Pancake.Common;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "variable_int.asset", menuName = "Pancake/Scriptable/Variables/int")]
    [EditorIcon("so_blue_variable")]
    public class IntVariable : ScriptableVariable<int>
    {
        [Tooltip("Clamps the value of this variable to a minimum and maximum.")] [SerializeField]
        private bool isClamped;

        public bool IsClamped => isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField, ShowIf(nameof(isClamped)), Indent]
        private Vector2Int minMax = new(0, 100);

        public Vector2Int MinMax { get => minMax; set => minMax = value; }
        public int Min { get => minMax.x; set => minMax.x = value; }
        public int Max { get => minMax.y; set => minMax.y = value; }

        /// <summary>
        /// Returns the percentage of the value between the minimum and maximum.
        /// </summary>
        public float Ratio => Mathf.InverseLerp(minMax.x, minMax.y, value);

        public override void Save()
        {
            Data.Save(Guid, Value);
            base.Save();
        }

        public override void Load()
        {
            base.Load();
            Value = Data.Load(Guid, InitialValue);
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

            if (value == PreviousValue) return;
            ValueChanged();
        }
#endif
    }
}