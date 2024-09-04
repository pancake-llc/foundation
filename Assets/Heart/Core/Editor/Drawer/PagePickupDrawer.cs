using Alchemy.Editor;
using Pancake;
using Pancake.Linq;
using Pancake.UI;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PancakeEditor
{
    [CustomAttributeDrawer(typeof(PagePickupAttribute))]
    public class PagePickupDrawer : AlchemyAttributeDrawer
    {
        public override void OnCreateElement()
        {
            // Create a container for the horizontal layout
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 2}};

            var label = new Label(ObjectNames.NicifyVariableName(SerializedProperty.name.ToCamelCase())) {style = {marginLeft = 3, marginRight = 0, flexGrow = 1}};

            var button = new Button
            {
                text = "Select type...",
                style =
                {
                    flexGrow = 1,
                    marginLeft = 0,
                    flexShrink = 0,
                    marginRight = -3,
                    backgroundColor = Uniform.Error,
                    color = Color.white
                }
            };

            if (!string.IsNullOrEmpty(SerializedProperty.stringValue))
            {
                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = type.GetAllSubClass<Popup>();
                    foreach (var t in result)
                    {
                        if (t.Name == SerializedProperty.stringValue)
                        {
                            button.text = SerializedProperty.stringValue;
                            button.style.backgroundColor = new Color(0f, 0.18f, 0.53f, 0.31f);
                            break;
                        }

                        button.text = "Failed load...";
                        button.style.backgroundColor = Uniform.Error;
                    }
                }
            }

            button.clicked += () =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None (-1)"),
                    false,
                    () =>
                    {
                        SetAndApplyProperty(SerializedProperty, string.Empty);
                        button.text = "Select type...";
                        button.style.backgroundColor = Uniform.Error;
                    });
                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = type.GetAllSubClass<Popup>().Filter(t => !t.Name.Equals("Page`1"));
                    for (var i = 0; i < result.Count; i++)
                    {
                        if (i == 0) menu.AddSeparator("");
                        int cachei = i;
                        menu.AddItem(new GUIContent($"{result[cachei].Name} ({cachei})"),
                            false,
                            () =>
                            {
                                SetAndApplyProperty(SerializedProperty, result[cachei].Name);
                                button.text = result[cachei].Name;
                                button.style.backgroundColor = new Color(0f, 0.18f, 0.53f, 0.31f);
                            });
                    }
                }

                menu.DropDown(new Rect(button.worldBound.position, Vector2.zero));
            };


            container.Add(label);
            container.Add(button);

            TargetElement.Clear();
            TargetElement.Add(container);
        }

        private static System.Type GetTypeByFullName()
        {
            TypeExtensions.FindTypeByFullName("Pancake.UI.Page", out var type);
            return type;
        }

        private void SetAndApplyProperty(SerializedProperty property, string value)
        {
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}