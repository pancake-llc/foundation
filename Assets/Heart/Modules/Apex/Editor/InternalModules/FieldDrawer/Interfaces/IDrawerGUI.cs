using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IDrawerGUI
    {
        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the element drawer GUI.</param>
        /// <param name="element">Reference of serialized element with drawer attribute.</param>
        /// <param name="label">Display label of serialized element.</param>
        void OnGUI(Rect position, SerializedField element, GUIContent label);
    }
}