namespace Obvious.Soap
{
    [System.Serializable]
    public abstract class ScriptableListBase : ScriptableBase
    {
        public virtual System.Type GetElementType { get; }
    }
}