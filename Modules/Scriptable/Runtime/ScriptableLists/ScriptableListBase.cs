namespace Pancake.Scriptable
{
    [System.Serializable]
    public abstract class ScriptableListBase : ScriptableBase
    {
        public virtual System.Type GetGenericType { get; }
    }
}