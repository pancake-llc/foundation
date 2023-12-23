using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// Implements the IFactory interface for non-abstract types.
    /// </summary>
    /// <typeparam name="T">Specifies the non-abstract type to create.</typeparam>
    public abstract class ScriptableFactory<T> : ScriptableObject, IFactory<T>
    {
        [SerializeField, TextArea(3, 6)] private string developerDescription;

        public abstract T Create();
    }
}