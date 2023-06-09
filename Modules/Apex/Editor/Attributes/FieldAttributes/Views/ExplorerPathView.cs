using UnityEditor;
using UnityEngine;

namespace Pancake.ApexEditor
{
    public abstract class ExplorerPathView : FieldView, ITypeValidationCallback
    {
        private static Texture FolderIcon;
        private static GUIStyle IconStyle;
        static ExplorerPathView() { FolderIcon = EditorGUIUtility.IconContent("Folder Icon").image; }

        /// <summary>
        /// Select new path.
        /// </summary>
        protected abstract string GetSeletedPath();

        /// <summary>
        /// Called before applying selected path to the property.
        /// </summary>
        /// <param name="path">Selected path.</param>
        protected virtual string ValidatePath(string path) { return path; }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the Serialized field.</param>
        /// <param name="serializedField">Serialized field with ViewAttribute.</param>
        /// <param name="label">Label of Serialized field.</param>
        public sealed override void OnGUI(Rect position, SerializedField serializedField, GUIContent label)
        {
            if (IconStyle == null)
            {
                IconStyle = new GUIStyle("IconButton");
            }

            position = EditorGUI.PrefixLabel(position, label);

            position.x += 20;
            position.width -= 20;
            serializedField.GetSerializedProperty().stringValue = EditorGUI.TextField(position, serializedField.GetSerializedProperty().stringValue);

            position.x -= 20;
            position.width -= 20;
            if (GUI.Button(position, FolderIcon, IconStyle))
            {
                string selectedPath = GetSeletedPath();
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    serializedField.GetSerializedProperty().stringValue = ValidatePath(selectedPath);
                }
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.String; }
    }
}