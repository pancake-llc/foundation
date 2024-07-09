using UnityEngine;

namespace PancakeEditor.Common
{
    public interface ISearchWindow
    {
        /// <summary>
        /// Open search window.
        /// </summary>
        /// <param name="position">Window position in screen space.</param>
        /// <param name="width">Requested width of the window. Set to 0.0f to use the default width.</param>
        /// <param name="height">Requested height of the window. Set to 0.0f to use the default height.</param>
        void Open(Vector2 position, float width = 0, float height = 0);
    }
}