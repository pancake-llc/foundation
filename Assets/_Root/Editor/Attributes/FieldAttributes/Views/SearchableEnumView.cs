using Pancake.Editor.Window.Searchable;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pancake.Editor
{
    [ViewTarget(typeof(SearchableEnumAttribute))]
    sealed class SearchableEnumView : FieldView, ITypeValidationCallback
    {
        private SearchableEnumAttribute attribute;

        /// <summary>
        /// Called once when initializing PropertyView.
        /// </summary>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="viewAttribute">ViewAttribute of serialized element.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void Initialize(SerializedField element, ViewAttribute viewAttribute, GUIContent label) { attribute = viewAttribute as SearchableEnumAttribute; }

        /// <summary>
        /// Called for drawing element view GUI.
        /// </summary>
        /// <param name="position">Position of the serialized element.</param>
        /// <param name="element">Serialized element with ViewAttribute.</param>
        /// <param name="label">Label of serialized element.</param>
        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            Rect labelPosition = EditorGUI.PrefixLabel(position, label);

            Rect popupPosition = new Rect(labelPosition.x, labelPosition.y, position.x + position.width - labelPosition.x, position.height);
            if (GUI.Button(popupPosition, element.GetEnumValue(), EditorStyles.popup))
            {
                SearchableWindow searchableWindow = SearchableWindow.Create();
                for (int i = 0; i < element.GetEnumValueCount(); i++)
                {
                    int indexCopy = i;
                    string enumName = element.GetEnumValue(indexCopy);

                    if (attribute.HideValues?.Any(v => v == enumName) ?? false)
                    {
                        continue;
                    }

                    bool isActive = !attribute.DisableValues?.Any(v => v == enumName) ?? true;

                    SearchItem item = new SearchItem(new GUIContent(enumName));
                    item.OnClickCallback += () => element.SetEnum(indexCopy);
                    searchableWindow.AddItem(item);
                }

                if (attribute.Sort)
                {
                    searchableWindow.Sort();
                }

                searchableWindow.ShowAsDropDown(popupPosition);
            }
        }

        /// <summary>
        /// Return true if this property valid the using with this attribute.
        /// If return false, this property attribute will be ignored.
        /// </summary>
        /// <param name="property">Reference of serialized property.</param>
        public bool IsValidProperty(SerializedProperty property) { return property.propertyType == SerializedPropertyType.Enum; }
    }
}