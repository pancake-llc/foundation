using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(FolderPathAttribute))]
    sealed class FolderPathView : FieldView, ITypeValidationCallback
    {
        private FolderPathAttribute attribute;
        private GUIContent content;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            attribute = viewAttribute as FolderPathAttribute;
            content = EditorGUIUtility.IconContent("Folder Icon");
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            Rect stringFieldPosition = new Rect(position.xMin + 17, position.y - 1, position.width - 17, position.height);
            element.serializedProperty.stringValue = EditorGUI.TextField(stringFieldPosition, element.serializedProperty.stringValue);

            Rect buttonPosition = new Rect(position.xMin, position.y, 20, position.height);
            if (GUI.Button(buttonPosition, content, "IconButton"))
            {
                string selectedPath = EditorUtility.OpenFolderPanel(attribute.Title, attribute.Folder, attribute.DefaultName);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (attribute.RelativePath && selectedPath.Contains("Assets"))
                    {
                        selectedPath = selectedPath.Remove(0, selectedPath.IndexOf("Assets"));
                    }

                    element.serializedProperty.stringValue = selectedPath;
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