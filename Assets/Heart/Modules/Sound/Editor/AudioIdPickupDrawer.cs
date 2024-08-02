using Alchemy.Editor;
using Pancake.Sound;
using PancakeEditor.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PancakeEditor.Sound
{
    [CustomAttributeDrawer(typeof(AudioPickupAttribute))]
    public class AudioIdPickupDrawer : AlchemyAttributeDrawer
    {
        public override void OnCreateElement()
        {
            if (SerializedProperty.type != nameof(AudioId)) return;
            var idProperty = SerializedProperty.FindPropertyRelative("id");
            var nameProperty = SerializedProperty.FindPropertyRelative("name");

            // Create a container for the horizontal layout
            var container = new VisualElement {style = {flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4}};

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

            // Set the initial text of the button
            if (!string.IsNullOrEmpty(idProperty.stringValue))
            {
                var allAudioAsset = ProjectDatabase.FindAll<AudioData>();
                foreach (var t in allAudioAsset)
                {
                    if (t.id == idProperty.stringValue)
                    {
                        button.text = nameProperty.stringValue;
                        button.style.backgroundColor = new Color(0.99f, 0.5f, 0.24f, 0.31f);
                        break;
                    }

                    button.text = "Failed load...";
                    button.style.backgroundColor = Uniform.Error;
                }
            }

            button.clicked += () =>
            {
                var menu = new GenericMenu();
                var allAudioAsset = ProjectDatabase.FindAll<AudioData>();
                menu.AddItem(new GUIContent("None (-1)"),
                    false,
                    () =>
                    {
                        SetAndApplyProperty(idProperty, string.Empty);
                        button.text = "Select type...";
                        button.style.backgroundColor = Uniform.Error;
                    });
                for (var i = 0; i < allAudioAsset.Count; i++)
                {
                    if (i == 0) menu.AddSeparator("");
                    int cachei = i;
                    menu.AddItem(new GUIContent($"{allAudioAsset[cachei].name} ({cachei})"),
                        false,
                        () =>
                        {
                            SetAndApplyProperty(idProperty, allAudioAsset[cachei].id);
                            SetAndApplyProperty(nameProperty, allAudioAsset[cachei].name);
                            button.text = allAudioAsset[cachei].name;
                            button.style.backgroundColor = new Color(0.99f, 0.5f, 0.24f, 0.31f);
                        });
                }

                menu.DropDown(new Rect(button.worldBound.position, Vector2.zero));
            };


            container.Add(label);
            container.Add(button);

            // Add the container to the target element, which will hide default UI
            TargetElement.Clear();
            TargetElement.Add(container);
        }

        private void SetAndApplyProperty(SerializedProperty property, string value)
        {
            property.stringValue = value;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}