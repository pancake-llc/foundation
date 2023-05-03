using UnityEngine;

namespace Pancake.ApexEditor
{
    public interface IDecoratorGUI
    {
        /// <summary>
        /// Called for rendering and handling GUI events.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the decorator GUI.</param>
        void OnGUI(Rect position);
    }
}