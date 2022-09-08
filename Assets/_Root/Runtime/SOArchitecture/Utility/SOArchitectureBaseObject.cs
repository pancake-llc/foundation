using UnityEngine;

namespace Pancake.SOA
{
    /// <summary>
    /// Base class for SOArchitecture assets
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public abstract class SOArchitectureBaseObject : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline] [SerializeField] private string description;
#endif
    } 
}