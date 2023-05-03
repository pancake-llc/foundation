using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IViewGUI
    {
        /// <summary>
        /// Called for rendering and handling view GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the element view GUI.</param>
        /// <param name="element">Reference of serialized element with view attribute.</param>
        /// <param name="label">Display label of serialized element.</param>
        void OnGUI(Rect position, SerializedField element, GUIContent label);
    }
}