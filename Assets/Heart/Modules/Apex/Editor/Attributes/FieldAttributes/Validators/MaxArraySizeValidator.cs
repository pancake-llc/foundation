using Pancake.Apex;
using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    [ValidatorTarget(typeof(MaxArraySizeAttribute))]
    public sealed class MaxArraySizeValidator : FieldValidator, ITypeValidationCallback
    {
        private int size;

        /// <summary>
        /// Called once when initializing element validator.
        /// </summary>
        /// <param name="element">Serialized element with ValidatorAttribute.</param>
        /// <param name="validatorAttribute">ValidatorAttribute of serialized element.</param>
        /// <param name="label">Label of serialized property.</param>
        public override void Initialize(SerializedField element, ValidatorAttribute validatorAttribute, GUIContent label)
        {
            MaxArraySizeAttribute attribute = validatorAttribute as MaxArraySizeAttribute;
            size = attribute.size;
        }

        /// <summary>
        /// Implement this method to make some validation of serialized element.
        /// </summary>
        /// <param name="element">Serialized element with validator attribute.</param>
        public override void Validate(SerializedField element)
        {
            if (element.GetArrayLength() > size)
            {
                element.ResizeArray(size);
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.isArray && property.propertyType == SerializedPropertyType.Generic; }
    }
}