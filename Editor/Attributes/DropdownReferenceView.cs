using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(DropdownReferenceAttribute))]
    sealed class DropdownReferenceView : FieldView, ITypeValidationCallback
    {
        private FoldoutContainer foldoutContainer;

        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label)
        {
            foldoutContainer = new FoldoutContainer(label.text, "Group", null)
            {
                onChildrenGUI = element.DrawChildren,
                getChildrenHeight = element.GetChildrenHeight,
                onMenuButtonClick = (position) =>
                {
                    foldoutContainer.IsExpanded(false);
                    ShowWindow(position, element);
                }
            };
        }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label) { foldoutContainer.OnGUI(position); }

        /// <summary>
        /// Get height which needed to draw property.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override float GetHeight(SerializedField element, GUIContent label) { return foldoutContainer.GetHeight(); }

        private void ShowWindow(Rect position, SerializedField element)
        {
            ExSearchWindow searchWindow = ScriptableObject.CreateInstance<ExSearchWindow>();

            Type type = element.GetManagedReferenceType();

            foreach (Type derivedType in type.GetAllSubClass())
            {
                if (derivedType != null)
                {
                    SearchContent searchContent = derivedType.GetCustomAttribute<SearchContent>();
                    if (searchContent?.Hidden ?? false) continue;

                    GUIContent content = new GUIContent(searchContent.name);

                    // if (SearchContentUtility.TryLoadContentImage(searchContent.Image, out Texture2D image))
                    // {
                    //     content.image = image;
                    // }

                    searchWindow.AddEntry(content, () => element.CreateManagedReference(derivedType));
                }
            }

            GUIContent resetContent = new GUIContent("Reset");
            resetContent.image = EditorGUIUtility.IconContent("P4_DeletedLocal@2x").image;
            searchWindow.AddEntry(resetContent, () => element.SetManagedReference(null));

            searchWindow.Open(position);
        }

        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.ManagedReference; }
    }
}