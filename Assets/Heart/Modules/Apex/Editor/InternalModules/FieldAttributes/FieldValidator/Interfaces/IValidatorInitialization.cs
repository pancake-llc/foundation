using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IValidatorInitialization
    {
        /// <summary>
        /// Called once when initializing PropertyValidator.
        /// </summary>
        /// <param name="property">Serialized element with ValidatorAttribute.</param>
        /// <param name="attribute">ValidatorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        void Initialize(SerializedField element, ValidatorAttribute attribute, GUIContent label);
    }
}