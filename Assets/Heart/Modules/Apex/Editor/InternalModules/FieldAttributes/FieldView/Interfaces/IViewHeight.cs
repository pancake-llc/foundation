using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IViewHeight
    {
        /// <summary>
        /// Get height which needed to element view.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        float GetHeight(SerializedField element, GUIContent label);
    }
}