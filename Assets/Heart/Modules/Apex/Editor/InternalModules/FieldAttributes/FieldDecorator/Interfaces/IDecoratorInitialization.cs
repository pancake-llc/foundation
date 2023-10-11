using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IDecoratorInitialization
    {
        /// <summary>
        /// Called once, before any other decorator calls, 
        /// when the editor becomes active or enabled.
        /// </summary>
        /// <param name="element">Serialized element reference with current decorator attribute.</param>
        /// <param name="attribute">Reference of serialized property decorator attribute.</param>
        /// <param name="label">Display label of serialized property.</param>
        void Initialize(SerializedField element, DecoratorAttribute attribute, GUIContent label);
    }
}