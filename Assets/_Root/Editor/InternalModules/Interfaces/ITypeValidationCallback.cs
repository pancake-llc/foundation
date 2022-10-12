using UnityEngine;
using UnityEditor;

namespace Pancake.Editor
{
    public interface ITypeValidationCallback
    {
        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        bool IsValidProperty(SerializedProperty property);
    }
}