using Pancake.Apex;
using UnityEngine;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Check element for the valid of the specified conditions.
    /// </summary>
    public abstract class FieldValidator : IValidatorInitialization, IValidateElement
    {
        /// <summary>
        /// Called once when initializing validator.
        /// </summary>
        /// <param name="serializedField">Serialized field with ValidatorAttribute.</param>
        /// <param name="validatorAttribute">ValidatorAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public virtual void Initialize(SerializedField serializedField, ValidatorAttribute validatorAttribute, GUIContent label) { }

        /// <summary>
        /// Implement this method to make some validation of serialized serialized field.
        /// </summary>
        /// <param name="serializedField">Serialized field with validator attribute.</param>
        public abstract void Validate(SerializedField serializedField);
    }
}