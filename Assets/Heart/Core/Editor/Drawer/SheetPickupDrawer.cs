using Pancake;
using Pancake.ApexEditor;
using Pancake.ExLib.Reflection;
using Pancake.Linq;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [ViewTarget(typeof(SheetPickupAttribute))]
    public class SheetPickupDrawer : FieldView, ITypeValidationCallback
    {
        private const string NAME_CLASS_INHERIT = "Pancake.UI.Sheet";

        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            string buttonLabel = "Select type...";

            if (!string.IsNullOrEmpty(element.GetString()))
            {
                if (TypeExtensions.TryFindTypeByFullName(NAME_CLASS_INHERIT, out var type))
                {
                    var result = type.GetAllSubClass();
                    foreach (var type1 in result)
                    {
                        if (type1.Name == element.GetString())
                        {
                            buttonLabel = element.GetString();
                            break;
                        }

                        buttonLabel = "Failed load...";
                    }
                }
            }

            if (GUI.Button(position, buttonLabel, EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("None (-1)"),
                    false,
                    () =>
                    {
                        element.SetString(string.Empty);
                        element.GetSerializedObject().ApplyModifiedProperties();
                    });

                if (TypeExtensions.TryFindTypeByFullName(NAME_CLASS_INHERIT, out var type))
                {
                    var result = type.GetAllSubClass();
                    result = result.Filter(t => !t.Name.Equals("Sheet`1"));
                    for (var i = 0; i < result.Count; i++)
                    {
                        if (i == 0) menu.AddSeparator("");
                        int cachei = i;
                        menu.AddItem(new GUIContent($"{result[i].Name} ({i})"),
                            false,
                            () =>
                            {
                                element.SetString(result[cachei].Name);
                                element.GetSerializedObject().ApplyModifiedProperties();
                            });
                    }
                }

                menu.DropDown(position);
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