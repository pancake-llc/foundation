using UnityEngine;

namespace Pancake.Editor
{
    public interface IVisualEntity
    {
        /// <summary>
        /// Called for rendering and handling visual entity.
        /// </summary>
        /// <param name="position">Rectangle position.</param>
        void OnGUI(Rect position);
    }
}