using System;

namespace Pancake.ApexEditor
{
    public interface IGUIChangedCallback
    {
        /// <summary>
        /// Called when GUI has been changed.
        /// </summary>
        event Action GUIChanged;
    }
}