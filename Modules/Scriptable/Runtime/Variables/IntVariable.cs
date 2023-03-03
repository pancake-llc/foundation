using UnityEngine;

namespace Pancake.Scriptable
{
    [CreateAssetMenu(fileName = "scriptable_variable_int.asset", menuName = "Pancake/Scriptable/Variables/int")]
    [System.Serializable]
    public class IntVariable : ScriptableVariable<int>
    {
        [SerializeField] private bool isClamped;

        public bool IsClamped => isClamped;

        [Tooltip("If clamped, sets the minimum and maximum")] [SerializeField] [ShowIf("isClamped", true)]
        private Vector2Int minMax = new Vector2Int(int.MinValue, int.MaxValue);

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

        public void Add(int value) { Value += value; }

        public override int Value
        {
            get => value;
            set
            {
                int clampedValue = IsClamped ? value.Clamp(minMax.x, minMax.y) : value;
                base.Value = clampedValue;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (IsClamped)
            {
                int clampedValue = value.Clamp(minMax.x, minMax.y);
                if (value < clampedValue || value > clampedValue) value = clampedValue;
            }

            base.OnValidate();
        }
#endif
    }
}