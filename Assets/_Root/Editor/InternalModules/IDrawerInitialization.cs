using UnityEngine;

namespace Pancake.Editor
{
    public interface IDrawerInitialization
    {
        /// <summary>
        /// Called once when initializing element drawer.
        /// </summary>
        /// <param name="element">Serialized element with DrawerAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        void Initialize(SerializedField element, GUIContent label);
    }
}