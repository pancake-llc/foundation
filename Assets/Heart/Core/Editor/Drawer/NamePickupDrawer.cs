using Pancake;
using Pancake.Apex;
using Pancake.ApexEditor;
using Pancake.ExLib.Reflection;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    [ViewTarget(typeof(NamePickupAttribute))]
    public sealed class NamePickupDrawer : FieldView, ITypeValidationCallback
    {
        private string _nameClassInherit;

        public override void Initialize(SerializedField serializedField, ViewAttribute viewAttribute, GUIContent label)
        {
            var attribute = viewAttribute as NamePickupAttribute;
            _nameClassInherit = attribute.NameClassInherit;
        }

        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            string buttonLabel = "Select type...";

            if (!string.IsNullOrEmpty(element.GetString()))
            {
                if (TypeExtensions.TryFindTypeByFullName(_nameClassInherit, out var type))
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

                if (TypeExtensions.TryFindTypeByFullName(_nameClassInherit, out var type))
                {
                    var result = type.GetAllSubClass();
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