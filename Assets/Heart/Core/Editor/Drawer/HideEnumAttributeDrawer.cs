using Alchemy.Editor;
using Pancake;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;
using PancakeEditor.Common;

namespace PancakeEditor
{
    [CustomAttributeDrawer(typeof(HideEnumAttribute))]
    public class HideEnumAttributeDrawer : AlchemyAttributeDrawer
    {
        public override void OnCreateElement()
        {
            // Create a container for the layout
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2}};

            var label = new Label(ObjectNames.NicifyVariableName(SerializedProperty.name)) {style = {marginLeft = 3, marginRight = 0, flexGrow = 1}};

            var enumType = TypeExtensions.GetFieldInfoFromProperty(SerializedProperty).FieldType;
            if (!enumType.IsEnum)
            {
                Debug.LogError("SerializedProperty is not an enum type.");
                return;
            }

            // Ensure attribute is not null
            if (Attribute is HideEnumAttribute hideEnumAttribute)
            {
                var hiddenValues = hideEnumAttribute.HiddenValues.Cast<Enum>().ToList();
                var displayedOptions = Enum.GetValues(enumType).Cast<Enum>().Where(e => !hiddenValues.Contains(e)).ToList();

                var currentEnumValue = (Enum) Enum.ToObject(enumType, SerializedProperty.enumValueIndex);
                int currentIndex = displayedOptions.IndexOf(currentEnumValue);

                // Create a PopupField to select from the filtered enum options
                var popupField = new PopupField<Enum>(string.Empty, displayedOptions, currentIndex) {style = {flexGrow = 1, marginLeft = 0, marginRight = -3}};

                // Update the SerializedProperty when a new value is selected
                popupField.RegisterValueChangedCallback(evt => { SetAndApplyProperty(SerializedProperty, evt.newValue); });

                container.Add(label);
                container.Add(popupField);
            }
            else
            {
                Debug.LogError("HideEnumAttribute is null or not correctly assigned.");
            }

            // Clear the TargetElement and add the custom container
            TargetElement.Clear();
            TargetElement.Add(container);
        }

        private void SetAndApplyProperty(SerializedProperty property, Enum value)
        {
            property.enumValueIndex = Convert.ToInt32(value);
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}