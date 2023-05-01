namespace Obvious.Soap
{
    [System.Serializable]
    public abstract class VariableReference<V, T> where V : ScriptableVariable<T>
    {
        public bool UseLocal = false;
        public T LocalValue;
        public V Variable;
        
        public T Value
        {
            get => UseLocal ? LocalValue : Variable.Value;
            set
            {
                if (UseLocal)
                    LocalValue = value;
                else
                    Variable.Value = value;
            }
        }

        public static implicit operator T(VariableReference<V, T> reference)
        {
            return reference.Value;
        }
    }
}