using Pancake.Apex;
using UnityEngine;
using UnityEditor;

namespace Pancake.ApexEditor
{
    /// <summary>
    /// Completely change how the property is drawing and how you interact with it.
    /// </summary>
    public abstract class FieldView : IViewInitialization, IViewGUI, IViewHeight
    {
        /// <summary>
        /// Called once when initializing FieldView.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of Serialized field.</param>
        /// <param name="label">Label of Serialized field.</param>
        public virtual void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label) { }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public abstract void OnGUI(Rect position, SerializedField serializedField, GUIContent label);

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public virtual float GetHeight(SerializedField serializedField, GUIContent label) { return EditorGUIUtility.singleLineHeight; }
    }
}