namespace Pancake.Scriptable
{
    [System.Serializable]
    public abstract class ScriptableEventBase : ScriptableBase
    {
        public virtual System.Type GetGenericType { get; }
    }
}