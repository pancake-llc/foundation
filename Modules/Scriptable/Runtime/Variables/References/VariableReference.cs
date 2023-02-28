namespace Pancake.Scriptable
{
    [System.Serializable]
    public class VariableReference<V, T> where V : ScriptableVariable<T>
    {
        public bool useLocal;
        public T localValue;
        public V variable;

        public T Value
        {
            get => useLocal ? localValue : variable.Value;
            set
            {
                if (useLocal) localValue = value;
                else variable.Value = value;
            }
        }

        public static implicit operator T(VariableReference<V, T> reference) { return reference.Value; }
    }
}