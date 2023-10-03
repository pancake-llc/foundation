using UnityEngine;

namespace Pancake.ExLibEditor.Windows
{
    /// <summary>
    /// Open search window.
    /// </summary>
    /// <param name="position">Window position in screen space.</param>
    /// <param name="width">Requested width of the window. Set to 0.0f to use the default width.</param>
    /// <param name="height">Requested height of the window. Set to 0.0f to use the default height.</param>
    public interface IExSearchWindow
    {
        void Open(Vector2 position, float width = 0, float height = 0);
    }
}