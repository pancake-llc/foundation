using Pancake;
using Pancake.ApexEditor;
using PancakeEditor.Common;

using Pancake.Linq;
using Pancake.UI;
using UnityEditor;
using UnityEngine;

namespace PancakeEditor
{
    //[ViewTarget(typeof(PopupPickupAttribute))]
    public sealed class PopupPickupDrawer : FieldView, ITypeValidationCallback
    {
        private const string NAME_CLASS_INHERIT = "Pancake.UI.Popup";

        public override void OnGUI(Rect position, SerializedField element, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            var buttonLabel = "Select type...";

            if (!string.IsNullOrEmpty(element.GetString()))
            {
                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = type.GetAllSubClass<Popup>();
                    foreach (var t in result)
                    {
                        if (t.Name == element.GetString())
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
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None (-1)"), false, () => SetAndApplyProperty(element, string.Empty));
                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = type.GetAllSubClass<Popup>().Filter(t => !t.Name.Equals("Popup`1"));
                    for (var i = 0; i < result.Count; i++)
                    {
                        if (i == 0) menu.AddSeparator("");
                        int cachei = i;
                        menu.AddItem(new GUIContent($"{result[i].Name} ({i})"), false, () => SetAndApplyProperty(element, result[cachei].Name));
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

        private static System.Type GetTypeByFullName()
        {
            TypeExtensions.TryFindTypeByFullName(NAME_CLASS_INHERIT, out var type);
            return type;
        }

        private static void SetAndApplyProperty(SerializedField element, string value)
        {
            element.SetString(value);
            element.GetSerializedObject().ApplyModifiedProperties();
        }
    }
}