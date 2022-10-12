using Pancake.Editor.Window.Searchable;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(DropdownReferenceAttribute))]
    sealed class DropdownReferenceView : FieldView
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
            SearchableWindow searchableWindow = ScriptableObject.CreateInstance<SearchableWindow>();

            Type type = element.GetManagedReferenceType();
            Type[] derivedTypes = InspectorReflection.FindSubclassesOf(type).ToArray();

            SearchItem emptyItem = new SearchItem(new GUIContent("Empty"));
            emptyItem.OnClickCallback += () => element.SetManagedReference(null);
            searchableWindow.AddItem(emptyItem);

            for (int i = 0; i < derivedTypes.Length; i++)
            {
                Type derivedType = derivedTypes[i];
                if (derivedType != null)
                {
                    ReferenceContent referenceContent = derivedType.GetCustomAttribute<ReferenceContent>();
                    if (referenceContent?.Hided ?? false)
                        continue;

                    SearchItem item = new SearchItem(new GUIContent(referenceContent?.GetName() ?? derivedType.Name), referenceContent?.Active ?? true);
                    item.OnClickCallback += () => element.CreateManagedReference(derivedType);
                    ;
                    searchableWindow.AddItem(item);
                }
            }

            searchableWindow.ShowAsDropDown(position);
        }
    }
}