using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IInlineDecoratorGUI
    {
        /// <summary>
        /// Called for rendering and handling inline decorator GUI.
        /// </summary>
        /// <param name="position">Calculated position for drawing inline decorator.</param>
        void OnGUI(Rect position);
    }
}