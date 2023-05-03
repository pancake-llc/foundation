using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public abstract class FieldDrawer : IDrawerInitialization, IDrawerGUI, IDrawerHeight
    {
        /// <summary>
        /// Called once when initializing serialized field drawer.
        /// </summary>
        /// <param name="serializedField">Serialized field with DrawerAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public virtual void Initialize(SerializedField serializedField, GUIContent label) { }

        /// <summary>
        /// Called for rendering and handling drawer GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the serialized field drawer GUI.</param>
        /// <param name="serializedField">Reference of serialized field with drawer attribute.</param>
        /// <param name="label">Display label of serialized field.</param>
        public abstract void OnGUI(Rect position, SerializedField serializedField, GUIContent label);

        /// <summary>
        /// Get height which needed to serialized field drawer.
        /// </summary>
        /// <param name="serializedField">Serialized field with DrawerAttribute.</param>
        /// <param name="label">Label of serialized field.</param>
        public virtual float GetHeight(SerializedField serializedField, GUIContent label) { return EditorGUIUtility.singleLineHeight; }
    }
}