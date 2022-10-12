using UnityEngine;
using UnityEditor;

namespace Pancake.Editor
{
    /// <summary>
    /// Completely change how the property is drawing and how you interact with it.
    /// </summary>
    public abstract class FieldView : IViewInitialization, IViewGUI, IViewHeight
    {
        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public virtual void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label) { }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public abstract void OnGUI(Rect position, SerializedField element, GUIContent label);

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public virtual float GetHeight(SerializedField element, GUIContent label) { return EditorGUIUtility.singleLineHeight; }
    }
}