namespace Pancake
{
    /// <summary>
    /// Implements the IFactory interface for non-abstract types.
    /// </summary>
    /// <typeparam name="T">Specifies the non-abstract type to create.</typeparam>
    public abstract class ScriptableFactory<T> : UnityEngine.ScriptableObject, IFactory<T>
    {
        public abstract T Create();
    }
}