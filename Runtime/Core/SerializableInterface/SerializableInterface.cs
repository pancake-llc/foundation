using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Pancake
{
    /// <summary>
    /// A wrapper around an interface that supports serialization for both UnityEngine.Object and regular object types
    /// </summary>
    /// <typeparam name="TInterface">The type of the interface you want to serialize</typeparam>
    [Serializable]
    public class SerializableInterface<TInterface> : ISerializableInterface where TInterface : class
    {
        [HideInInspector, SerializeField] private InterfaceRefMode mode = InterfaceRefMode.Unity;
        [HideInInspector, SerializeField] private UnityEngine.Object unityReference;
        [SerializeReference, UsedImplicitly] private object rawReference;

        public TInterface Value
        {
            get
            {
                return mode switch
                {
                    InterfaceRefMode.Raw => rawReference as TInterface,
                    InterfaceRefMode.Unity => (object)unityReference as TInterface,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            set
            {
                if (value is UnityEngine.Object unityObject)
                {
                    rawReference = null;
                    unityReference = unityObject;
                    mode = InterfaceRefMode.Unity;
                }
                else
                {
                    unityReference = null;
                    rawReference = value;
                    mode = InterfaceRefMode.Raw;
                }
            }
        }

        /// <inheritdoc />
        object ISerializableInterface.GetRawReference()
        {
            return rawReference;
        }
    }
}
