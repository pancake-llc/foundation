using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IDrawerHeight
    {
        /// <summary>
        /// Get height which needed to element drawer.
        /// </summary>
        /// <param name="element">Serialized element with DrawerAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        float GetHeight(SerializedField element, GUIContent label);
    }
}