using UnityEngine;

namespace Pancake.Editor
{
    /// <summary>
    /// Check element for the valid of the specified conditions.
    /// </summary>
    public abstract class FieldValidator : IValidatorInitialization, IValidateElement
    {
        /// <summary>
        /// Called once when initializing element validator.
        /// </summary>
        /// <param name="property">Serialized element with ValidatorAttribute.</param>
        /// <param name="attribute">ValidatorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        public virtual void Initialize(SerializedField element, ValidatorAttribute validatorAttribute, GUIContent label) { }

        /// <summary>
        /// Implement this method to make some validation of serialized element.
        /// </summary>
        /// <param name="element">Serialized element with validator attribute.</param>
        public abstract void Validate(SerializedField element);
    }
}