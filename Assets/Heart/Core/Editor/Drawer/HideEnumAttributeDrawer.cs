using System;
using System.Linq;
using Pancake;
using UnityEditor;
using UnityEngine.UIElements;

namespace PancakeEditor
{
    [CustomPropertyDrawer(typeof(HideEnumAttribute))]
    public sealed class HideEnumPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            var enumType = fieldInfo.FieldType;
            var hideEnumAttribute = (HideEnumAttribute) attribute;
            var hiddenValues = hideEnumAttribute.HiddenValues.Cast<Enum>();

            var displayedOptions = Enum.GetValues(enumType).Cast<Enum>().Where(e => !hiddenValues.Contains(e)).ToList();

            var currentEnumValue = (Enum) Enum.ToObject(enumType, property.enumValueIndex);
            int currentIndex = displayedOptions.IndexOf(currentEnumValue);

            var popupField = new PopupField<Enum>(property.displayName, displayedOptions, currentIndex)
            {
                formatListItemCallback = item => item.ToString(), formatSelectedValueCallback = item => item.ToString()
            };

            popupField.RegisterValueChangedCallback(evt =>
            {
                property.enumValueIndex = Array.IndexOf(Enum.GetValues(enumType), evt.newValue);
                property.serializedObject.ApplyModifiedProperties();
            });

            container.Add(popupField);

            return container;
        }
    }
}