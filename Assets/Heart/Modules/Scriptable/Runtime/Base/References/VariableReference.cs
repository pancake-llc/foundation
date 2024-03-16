namespace Pancake.Scriptable
{
    [System.Serializable]
    public abstract class VariableReference<TV, TL> where TV : ScriptableVariable<TL>
    {
        public bool useLocal;
        public TL localValue;
        public TV variable;

        public TL Value
        {
            get => useLocal ? localValue : variable.Value;
            set
            {
                if (useLocal) localValue = value;
                else variable.Value = value;
            }
        }

        public static implicit operator TL(VariableReference<TV, TL> reference) { return reference.Value; }
    }
}