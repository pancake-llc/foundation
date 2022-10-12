using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    public abstract class FieldDrawer : IDrawerInitialization, IDrawerGUI, IDrawerHeight
    {
        /// <summary>
        /// Called once when initializing element drawer.
        /// </summary>
        /// <param name="element">Serialized element with DrawerAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public virtual void Initialize(SerializedField element, GUIContent label) { }

        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the element drawer GUI.</param>
        /// <param name="element">Reference of serialized element with drawer attribute.</param>
        /// <param name="label">Display label of serialized element.</param>
        public abstract void OnGUI(Rect position, SerializedField element, GUIContent label);

        /// <summary>
        /// Get height which needed to element drawer.
        /// </summary>
        /// <param name="element">Serialized element with DrawerAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public virtual float GetHeight(SerializedField element, GUIContent label) { return EditorGUIUtility.singleLineHeight; }
    }
}