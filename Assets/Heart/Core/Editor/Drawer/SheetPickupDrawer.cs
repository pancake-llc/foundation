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
    [CustomAttributeDrawer(typeof(SheetPickupAttribute))]
    public class SheetPickupDrawer : AlchemyAttributeDrawer
    {
        public override void OnCreateElement()
        {
            // Create a container for the horizontal layout
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4}};

            var label = new Label(ObjectNames.NicifyVariableName(SerializedProperty.name.ToCamelCase())) {style = {marginLeft = 3, marginRight = 10, flexGrow = 0}};

            var button = new Button
            {
                text = "Select type...",
                style =
                {
                    flexGrow = 1,
                    marginLeft = 10,
                    flexShrink = 0,
                    marginRight = -3,
                    backgroundColor = new Color(0.93f, 0.78f, 0.22f, 0.31f),
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
                        if (t.Namespace == SerializedProperty.stringValue)
                        {
                            button.text = SerializedProperty.stringValue;
                            break;
                        }

                        button.text = "Failed load...";
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
                    });
                var type = GetTypeByFullName();
                if (type != null)
                {
                    var result = type.GetAllSubClass<Popup>().Filter(t => !t.Name.Equals("Sheet`1"));
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
            TypeExtensions.TryFindTypeByFullName("Pancake.UI.Sheet", out var type);
            return type;
        }

        private void SetAndApplyProperty(SerializedProperty property, string value)
        {
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}